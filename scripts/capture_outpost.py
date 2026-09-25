#!/usr/bin/env python3
"""Capture Editor-rendered blockout views and a physical traversal in disposable state."""
import argparse
import shutil
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=240)
    args = parser.parse_args(argv)
    evidence = unity.new_evidence("outpost-graphics")
    project = evidence / "project"
    save = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        save = unity.isolate_save_identity(project)
        command = [str(editor), "-batchmode", "-quit", "-projectPath", str(project),
                   "-executeMethod", "CommonHouse.Editor.OutpostEvidence.Capture",
                   "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, args.timeout, "test")
        if code:
            raise ValueError(f"Graphical Editor run failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)
        required = ["outpost-cutaway.png", "traversal-observation.txt", "graphics-device.txt"]
        required += [f"outpost-traverse-{index}.png" for index in range(5)]
        for name in required:
            path = evidence / "test-data" / name
            if not path.is_file() or path.stat().st_size == 0:
                raise ValueError(f"Missing evidence artifact: {name}")
        unity.receipt(evidence, status="passed", editor_version=version, command=command,
                      artifacts=required, disposable_save_path=str(save))
        artifact_directory = evidence / "test-data"
        print(f"PASS: Editor-rendered captures and physical traversal. Inspect images at {artifact_directory}")
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
