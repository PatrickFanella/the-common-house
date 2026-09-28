#!/usr/bin/env python3
"""Render the PS1 outpost from the N64 comparison views in a disposable project copy (issue #69)."""
import argparse
import shutil
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity

# Must match N64Outpost.Views, which PS1Outpost.Capture reuses.
VIEWS = ["corridor", "resident", "radio-desk", "kitchen", "archive-shelf", "workshop"]


def artifacts():
    names = []
    for view in VIEWS:
        names += [f"{view}-ps1.png", f"{view}-ps1-display.png"]
    return names


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=600)
    args = parser.parse_args(argv)
    evidence = unity.new_evidence("ps1")
    project = evidence / "project"
    save = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        save = unity.isolate_save_identity(project)
        command = [str(editor), "-batchmode", "-quit", "-projectPath", str(project),
                   "-executeMethod", "CommonHouse.Editor.Experiments.PS1Outpost.Capture",
                   "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, args.timeout, "test")
        if code:
            raise ValueError(f"Graphical Editor run failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)
        required = artifacts()
        for name in required:
            path = evidence / "test-data" / name
            if not path.is_file() or path.stat().st_size == 0:
                raise ValueError(f"Missing PS1 artifact: {name}")
        unity.receipt(evidence, status="passed", editor_version=version, command=command,
                      artifacts=required, disposable_save_path=str(save))
        print(f"PASS: PS1 renders available for inspection at {evidence / 'test-data'}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error), disposable_save_path=str(save) if save else None)
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)
        if save is not None and save.exists():
            shutil.rmtree(save)


if __name__ == "__main__":
    raise SystemExit(main())
