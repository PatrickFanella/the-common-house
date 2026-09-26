#!/usr/bin/env python3
"""Run explicit Editor methods on a disposable copy and optionally copy generated sources back.

Use for one-time configuration and scene generation. The copy-back step replaces only
Assets/, Packages/, and ProjectSettings/ in this checkout; review the Git diff afterwards.
"""
import argparse
import shutil
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity

SOURCES = ["Assets", "Packages", "ProjectSettings"]


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("methods", nargs="+", help="Fully qualified static methods, run in separate Editor sessions.")
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=900)
    parser.add_argument("--copy-back", action="store_true", help="Replace checkout sources with the generated copy.")
    args = parser.parse_args(argv)
    evidence = unity.new_evidence("editor-method")
    project = evidence / "project"
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        for index, method in enumerate(args.methods):
            log = evidence / f"editor-{index}.log"
            command = [str(editor), "-batchmode", "-quit", "-force-vulkan", "-projectPath", str(project),
                       "-executeMethod", method, "-logFile", str(log)]
            code = unity.execute(command, evidence, args.timeout, "development")
            if code:
                raise ValueError(f"{method} failed with exit {code}")
            unity.validate_log(log, require_import=True)
        if args.copy_back:
            for name in SOURCES:
                shutil.rmtree(unity.ROOT / name)
                shutil.copytree(project / name, unity.ROOT / name, symlinks=False)
        unity.receipt(evidence, status="passed", editor_version=version, methods=args.methods, copied_back=args.copy_back)
        print(f"PASS: {', '.join(args.methods)}. Evidence: {evidence}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error))
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)


if __name__ == "__main__":
    raise SystemExit(main())
