#!/usr/bin/env python3
"""Read-only Unity inventory. Does not inspect credentials or activate licenses."""
import argparse
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import platform
import re
import shutil
import subprocess
import sys

VERSION = re.compile(r"\d+\.\d+\.\d+[abfp]\d+")


def probe(argv):
    try:
        result = subprocess.run(argv, capture_output=True, text=True, timeout=20)
        return result.returncode, result.stdout
    except (OSError, subprocess.TimeoutExpired):
        return None, ""


def candidates(home, explicit=None):
    paths = [Path(explicit).expanduser()] if explicit else []
    for root in [home / "Unity/Hub/Editor", home / "Unity", Path("/opt/unity/editors")]:
        paths.extend(sorted(root.glob("*/Editor/Unity")))
    paths.extend([Path("/opt/Unity/Editor/Unity"), Path("/opt/unity/Editor/Unity")])
    return list(dict.fromkeys(paths))


def inspect(home=Path.home(), explicit=None):
    checked = candidates(home, explicit)
    editor = None
    for path in checked:
        if not path.is_file() or not os.access(path, os.X_OK):
            continue
        code, output = probe([str(path), "-version"])
        version = VERSION.fullmatch(output.strip())
        if code == 0 and version:
            support = path.parent / "Data/PlaybackEngines"
            editor = {"path": str(path.resolve()), "version": version[0],
                      "version_exit_code": code,
                      "build_support": sorted(p.name for p in support.iterdir() if p.is_dir()) if support.is_dir() else []}
            break
    os_info = {}
    for line in Path("/etc/os-release").read_text().splitlines():
        key, _, value = line.partition("=")
        if key in {"NAME", "VERSION_ID", "ID"}:
            os_info[key] = value.strip('"')
    memory = re.search(r"^MemTotal:\s+(\d+) kB$", Path("/proc/meminfo").read_text(), re.M)
    gpu_code, gpu_output = probe(["lspci", "-mm"])
    gpu = [line.split(" ", 1)[1] for line in gpu_output.splitlines()
           if any(kind in line for kind in ['"VGA compatible controller"', '"3D controller"', '"Display controller"'])]
    blockers = [] if editor else ["No working Editor found in the checked paths; this is not an exhaustive filesystem search."]
    if not memory or not gpu:
        blockers.append("Hardware inventory is incomplete; inspect missing fields before choosing graphics/build settings.")
    return {
        "schema_version": 1, "observed_at": datetime.now(timezone.utc).isoformat(),
        "host": platform.node(), "os": {**os_info, "kernel": platform.release(), "architecture": platform.machine()},
        "memory_bytes": int(memory[1]) * 1024 if memory else None,
        "gpu": {"devices": gpu, "exit_code": gpu_code}, "hub_path": shutil.which("unityhub"),
        "editor": editor, "checked_editor_paths": [str(p) for p in checked],
        "readiness": {"state": "blocked" if blockers else "editor-found-license-unverified",
                      "blockers": blockers, "license": "unverified", "project_import": "not-run"},
        "provenance": {"os": "/etc/os-release and Python platform", "memory": "/proc/meminfo:MemTotal",
                       "gpu": ["lspci", "-mm"], "editor": [editor["path"], "-version"] if editor else None,
                       "build_support": "Editor/Data/PlaybackEngines directory names only"},
        "limitations": ["Build module presence is not a successful build.",
                        "Version output does not prove license entitlement, project import, or GPU rendering.",
                        "No account, license file, token, or Hub session data was read."]}


def validate(report):
    try:
        if report["schema_version"] != 1 or not report["host"]:
            raise ValueError("Missing schema version or host")
        observed = datetime.fromisoformat(report["observed_at"])
        if observed.tzinfo is None:
            raise ValueError("Observation must include timezone")
        for name in ["os", "memory", "gpu", "build_support"]:
            if not report["provenance"][name]:
                raise ValueError(f"Missing {name} provenance")
        if not report["os"].get("NAME") or not isinstance(report["gpu"]["devices"], list):
            raise ValueError("Missing OS/GPU inventory")
        status = report["readiness"]
        if status["license"] != "unverified" or status["project_import"] != "not-run":
            raise ValueError("Read-only inventory cannot certify licensing or import")
        if status["state"] not in {"blocked", "editor-found-license-unverified"}:
            raise ValueError("Unsupported readiness claim")
        if not isinstance(status["blockers"], list):
            raise ValueError("Blockers must be a list")
        editor = report["editor"]
        if editor:
            if not Path(editor["path"]).is_absolute() or not VERSION.fullmatch(editor["version"]) or editor["version_exit_code"] != 0:
                raise ValueError("Invalid Editor evidence")
            if report["provenance"]["editor"] != [editor["path"], "-version"] or not isinstance(editor["build_support"], list):
                raise ValueError("Missing Editor provenance")
        if status["state"] == "editor-found-license-unverified":
            if not editor or status["blockers"] or not report["memory_bytes"] or not report["gpu"]["devices"]:
                raise ValueError("Incomplete inventory cannot claim Editor readiness")
        elif not status["blockers"]:
            raise ValueError("Blocked inventory needs a reason")
        if not report["checked_editor_paths"] or not report["limitations"]:
            raise ValueError("Missing search boundary or limitations")
    except (KeyError, TypeError, AttributeError) as error:
        raise ValueError("Malformed readiness report") from error


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--editor", help="Explicit Editor executable outside standard install paths")
    parser.add_argument("--output", type=Path, help="Write a redacted JSON inventory")
    parser.add_argument("--check-report", type=Path)
    args = parser.parse_args(argv)
    try:
        report = json.loads(args.check_report.read_text()) if args.check_report else inspect(explicit=args.editor)
        validate(report)
        if args.check_report:
            print(f"Valid inventory: {report['readiness']['state']}; licensing/import remain unverified")
        elif args.output:
            args.output.parent.mkdir(parents=True, exist_ok=True)
            args.output.write_text(json.dumps(report, indent=2) + "\n")
            print(f"Wrote inventory: {args.output} ({report['readiness']['state']})")
        else:
            print(json.dumps(report, indent=2))
        return 0
    except (ValueError, OSError) as error:
        print(str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
