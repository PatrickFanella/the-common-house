"""Shared native Unity checks using disposable source snapshots and private logs."""
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import tempfile
import uuid

ROOT = Path(__file__).resolve().parents[1]
VERSION = re.compile(r"\d+\.\d+\.\d+[abfp]\d+")
FAILURES = re.compile(r"error CS\d+|Scripts have compiler errors|Aborting batchmode due to failure|No valid Unity Editor license", re.I)


def resolve_editor(root=ROOT, explicit=None):
    pin = (root / "ProjectSettings/ProjectVersion.txt").read_text()
    match = re.search(r"^m_EditorVersion: (\S+)$", pin, re.M)
    if not match or not VERSION.fullmatch(match[1]):
        raise ValueError("Missing or invalid generated ProjectVersion.txt")
    supplied = explicit or os.environ.get("UNITY_EDITOR")
    if not supplied:
        inventory = root / "docs/development/unity-readiness.json"
        if inventory.is_file():
            supplied = (json.loads(inventory.read_text()).get("editor") or {}).get("path")
    if not supplied:
        raise ValueError("Set UNITY_EDITOR or pass --editor to the installed pinned Editor")
    path = Path(supplied).expanduser().resolve()
    if not path.is_file() or not os.access(path, os.X_OK):
        raise ValueError("Pinned Unity Editor executable is unavailable")
    try:
        result = subprocess.run([str(path), "-version"], capture_output=True, text=True, timeout=20)
    except subprocess.TimeoutExpired as error:
        raise ValueError("Editor version check timed out") from error
    if result.returncode or result.stdout.strip() != match[1]:
        raise ValueError("Editor does not match the generated project version")
    return path, match[1]


def new_evidence(kind):
    base = Path.home() / ".local/state/common-house-unity"
    base.mkdir(parents=True, exist_ok=True, mode=0o700)
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    return Path(tempfile.mkdtemp(prefix=f"{kind}-{stamp}-", dir=base))


def snapshot(root, destination):
    """Copy only project sources. Reject links and outside local package references."""
    manifest = json.loads((root / "Packages/manifest.json").read_text())
    if manifest.get("scopedRegistries"):
        raise ValueError("Scoped registries require an explicit reviewed runner configuration")
    for value in manifest.get("dependencies", {}).values():
        if not isinstance(value, str) or not re.fullmatch(r"\d+\.\d+\.\d+(?:[-+.][A-Za-z0-9.-]+)?", value):
            raise ValueError("Runner accepts pinned registry/built-in packages only; review local or Git package access first")
    hashes = {}
    destination.mkdir()
    for name in ["Assets", "Packages", "ProjectSettings"]:
        source = root / name
        if not source.is_dir() or source.is_symlink():
            raise ValueError(f"Missing project directory or unsupported symlink: {name}")
        for path in source.rglob("*"):
            if path.is_symlink():
                raise ValueError("Project source contains a symlink; snapshots must not reach other host data")
            if path.is_file():
                relative = path.relative_to(root)
                if path.name.startswith(".env") or path.suffix.lower() in {".key", ".pem", ".pfx", ".p12"}:
                    raise ValueError("Credential-like file in project sources; remove it before verification")
                target = destination / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(path, target)
                hashes[str(relative)] = hashlib.sha256(path.read_bytes()).hexdigest()
        (destination / name).mkdir(exist_ok=True)
    return hashes


def development_env(evidence, mode):
    # Keep normal local licensing/session access without copying account files.
    allowed = {"HOME", "USER", "LOGNAME", "PATH", "DISPLAY", "WAYLAND_DISPLAY",
               "XAUTHORITY", "XDG_RUNTIME_DIR", "XDG_DATA_HOME", "XDG_CONFIG_HOME",
               "DBUS_SESSION_BUS_ADDRESS", "LANG", "LC_ALL"}
    env = {key: value for key, value in os.environ.items() if key in allowed}
    for name in ["config", "data", "cache", "test-data"]:
        (evidence / name).mkdir(exist_ok=True)
    env.update(XDG_CACHE_HOME=str(evidence / "cache"), COMMON_HOUSE_ENV=mode,
               COMMON_HOUSE_TEST_DATA_PATH=str(evidence / "test-data"))
    return env


def isolate_save_identity(project):
    """Give this copied project a unique Linux persistent-data namespace."""
    settings = project / "ProjectSettings/ProjectSettings.asset"
    text = settings.read_text()
    product = "run-" + uuid.uuid4().hex
    for key, value in [("companyName", "CommonHouseVerification"), ("productName", product)]:
        text, count = re.subn(rf"^  {key}:.*$", f"  {key}: {value}", text, flags=re.M)
        if count != 1:
            raise ValueError(f"Cannot isolate save identity: missing/duplicate {key}")
    config = Path(os.environ.get("XDG_CONFIG_HOME", str(Path.home() / ".config")))
    save_path = config / "unity3d/CommonHouseVerification" / product
    if save_path.exists():
        raise ValueError("Refusing to reuse an existing verification save directory")
    settings.write_text(text)
    return save_path


def execute(argv, evidence, timeout, mode):
    if timeout <= 0:
        raise ValueError("Timeout must be positive")
    (evidence / "command.json").write_text(json.dumps(argv, indent=2) + "\n")
    with (evidence / "console.log").open("w") as output:
        process = subprocess.Popen(argv, stdout=output, stderr=subprocess.STDOUT,
                                   env=development_env(evidence, mode), start_new_session=True)
        try:
            return process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            return 124
        except BaseException:
            if process.poll() is None:
                os.killpg(process.pid, signal.SIGKILL)
                process.wait()
            raise


def validate_log(path, require_import=False):
    if not path.is_file():
        raise ValueError("Editor did not produce a log")
    text = path.read_text(errors="replace")
    if FAILURES.search(text):
        raise ValueError("Editor log contains a compilation, licensing, or batch failure; inspect private evidence")
    if require_import and "Exiting batchmode successfully now!" not in text:
        raise ValueError("Editor log has no successful batch completion marker")


def receipt(evidence, **values):
    (evidence / "result.json").write_text(json.dumps(values, indent=2) + "\n")


def record_source(evidence, hashes):
    revision = subprocess.run(["git", "-C", str(ROOT), "rev-parse", "HEAD"], capture_output=True, text=True)
    dirty = subprocess.run(["git", "-C", str(ROOT), "status", "--porcelain"], capture_output=True, text=True)
    (evidence / "source.json").write_text(json.dumps({
        "source": str(ROOT), "revision": revision.stdout.strip() if revision.returncode == 0 else None,
        "dirty": dirty.stdout.splitlines() if dirty.returncode == 0 else None,
        "source_hashes": hashes,
        "snapshot_overrides": "Unique company/product names isolate Linux persistent data; licensing uses normal local context."
    }, indent=2) + "\n")
