#!/usr/bin/env python3
"""Import a disposable copy with the exact pinned Unity Editor."""
import argparse
from pathlib import Path
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
    evidence = unity.new_evidence("import")
    project = evidence / "project"
    save_path = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        hashes = unity.snapshot(unity.ROOT, project)
        unity.record_source(evidence, hashes)
        save_path = unity.isolate_save_identity(project)
        command = [str(editor), "-batchmode", "-nographics", "-quit", "-projectPath", str(project),
                   "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, args.timeout, "development")
        if code:
            raise ValueError(f"Unity import failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)
        unity.receipt(evidence, status="passed", editor_version=version, command=command, source=str(unity.ROOT), source_hashes=hashes, disposable_save_path=str(save_path))
        print(f"PASS: Unity {version} source import. Evidence: {evidence}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error))
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)
        if save_path is not None and save_path.exists():
            shutil.rmtree(save_path)


if __name__ == "__main__":
    raise SystemExit(main())
