#!/usr/bin/env python3
"""Build the pinned Linux POC in disposable state and optionally smoke the actual player."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity


def output_manifest(root):
    if not root.is_dir():
        raise ValueError("Build output directory is missing")
    entries = []
    for path in sorted(root.rglob("*")):
        if path.is_symlink():
            raise ValueError("Build output contains an unsupported symlink")
        if path.is_file():
            relative = path.relative_to(root).as_posix()
            entries.append({"path": relative, "bytes": path.stat().st_size,
                            "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    if not entries:
        raise ValueError("Build output contains no files")
    digest = hashlib.sha256(json.dumps(entries, sort_keys=True, separators=(",", ":")).encode()).hexdigest()
    return {"sha256": digest, "files": entries}


def require_json(path, label):
    if not path.is_file():
        raise ValueError(f"Missing {label}")
    try:
        return json.loads(path.read_text())
    except json.JSONDecodeError as error:
        raise ValueError(f"Malformed {label}") from error


def run_player(executable, evidence, timeout):
    player_log = evidence / "player.log"
    command = [str(executable), "-screen-width", "1280", "-screen-height", "720", "-screen-fullscreen", "0",
               "-logFile", str(player_log), "--common-house-smoke=r2"]
    (evidence / "player-command.json").write_text(json.dumps(command, indent=2) + "\n")
    code = unity.execute(command, evidence, timeout, "test")
    if code:
        raise ValueError(f"Player smoke process failed with exit {code}")
    unity.validate_log(player_log)
    return command


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--target", choices=["local"], required=True)
    parser.add_argument("--smoke", action="store_true", help="Launch the actual Linux player using the explicit disposable R2 route.")
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=300)
    args = parser.parse_args(argv)
    if args.timeout <= 0:
        parser.error("Provide a positive timeout")

    evidence = unity.new_evidence("local-poc-build")
    project = evidence / "project"
    save_path = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        hashes = unity.snapshot(unity.ROOT, project)
        unity.record_source(evidence, hashes)
        save_path = unity.isolate_save_identity(project)
        build_command = [str(editor), "-batchmode", "-nographics", "-quit", "-projectPath", str(project),
                         "-executeMethod", "CommonHouse.Editor.LocalPocBuild.BuildLinux64", "-logFile", str(evidence / "editor.log")]
        (evidence / "build-command.json").write_text(json.dumps(build_command, indent=2) + "\n")
        code = unity.execute(build_command, evidence, args.timeout, "test")
        if (evidence / "console.log").is_file(): (evidence / "console.log").rename(evidence / "build-console.log")
        if (evidence / "command.json").is_file(): (evidence / "command.json").rename(evidence / "build-executed-command.json")
        if code:
            raise ValueError(f"Linux standalone build failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)

        output = evidence / "test-data" / "player"
        executable = output / "TheCommonHouse.x86_64"
        if not executable.is_file() or not executable.stat().st_mode & 0o111:
            raise ValueError("Linux standalone executable is missing or not executable")
        if not (output / "TheCommonHouse_Data").is_dir():
            raise ValueError("Linux standalone player data directory is missing")
        editor_receipt = require_json(output / "build-editor-receipt.json", "Editor build receipt")
        manifest = output_manifest(output)
        (evidence / "build-output-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")

        player_command = None
        smoke_receipt = None
        if args.smoke:
            player_command = run_player(executable, evidence, args.timeout)
            if (evidence / "console.log").is_file(): (evidence / "console.log").rename(evidence / "player-console.log")
            if (evidence / "command.json").is_file(): (evidence / "command.json").rename(evidence / "player-executed-command.json")
            smoke_root = evidence / "test-data" / "smoke"
            smoke_receipt = require_json(smoke_root / "smoke-result.json", "player smoke receipt")
            if smoke_receipt.get("status") != "passed" or smoke_receipt.get("route") != "R2":
                raise ValueError("Player smoke did not report a successful R2 route")
            screenshot = Path(smoke_receipt.get("screenshot", ""))
            if not screenshot.is_file() or screenshot.stat().st_size == 0:
                raise ValueError("Player smoke screenshot is missing")
            if smoke_receipt.get("outgoing_disclosures") != 1:
                raise ValueError("Player smoke did not preserve exactly one outgoing R2 disclosure")
            runtime_save = Path(smoke_receipt.get("save_path", ""))
            runtime_persistent = Path(smoke_receipt.get("persistent_data_path", ""))
            if not runtime_save.is_relative_to(save_path) or not runtime_persistent.is_relative_to(save_path):
                raise ValueError("Player smoke did not use the expected isolated persistent-data identity")
            if smoke_receipt.get("screen_width") != 1280 or smoke_receipt.get("screen_height") != 720:
                raise ValueError("Player smoke did not use the requested window size")
            if output_manifest(output) != manifest:
                raise ValueError("Player smoke changed the hashed build output")

        unity.receipt(evidence, status="passed", target="StandaloneLinux64", editor_version=version,
                      source_hashes=hashes, output_manifest_sha256=manifest["sha256"], output_files=len(manifest["files"]),
                      editor_receipt=editor_receipt, build_command=build_command, player_command=player_command,
                      smoke=smoke_receipt, disposable_save_path=str(save_path))
        print(f"PASS: Linux local POC build{' and R2 player smoke' if args.smoke else ''}. Evidence: {evidence}")
        return 0
    except (OSError, ValueError, subprocess.SubprocessError) as error:
        unity.receipt(evidence, status="failed", target="StandaloneLinux64", error=str(error),
                      disposable_save_path=str(save_path) if save_path else None)
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)
        if save_path is not None and save_path.exists():
            shutil.rmtree(save_path)


if __name__ == "__main__":
    raise SystemExit(main())
