#!/usr/bin/env python3
"""Run focused Unity tests in a disposable project; require actual passing XML."""
import argparse
import re
import shutil
import sys
import xml.etree.ElementTree as ET

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity


def validate_results(path, requested_filter=None):
    if not path.is_file():
        raise ValueError("Unity produced no test-results XML")
    try:
        root = ET.parse(path).getroot()
        total = int(root.attrib["total"])
        passed = int(root.attrib["passed"])
        failed = int(root.attrib["failed"])
        skipped = int(root.attrib["skipped"])
        inconclusive = int(root.attrib["inconclusive"])
    except (ET.ParseError, KeyError, ValueError) as error:
        raise ValueError("Malformed or incomplete Unity test-results XML") from error
    cases = root.findall(".//test-case")
    if root.tag != "test-run" or total <= 0 or not cases:
        raise ValueError("Zero matching tests or unsupported result format")
    if root.attrib.get("result") != "Passed" or failed or skipped or inconclusive or passed != total:
        raise ValueError(f"Tests did not all pass: total={total}, passed={passed}, failed={failed}, skipped={skipped}")
    if len(cases) != total or any(case.attrib.get("result") != "Passed" for case in cases):
        raise ValueError("Individual test results disagree with successful summary")
    if requested_filter:
        for case in cases:
            name = case.attrib.get("fullname", "")
            if not (name == requested_filter or name.startswith(requested_filter + ".") or name.startswith(requested_filter + "(")):
                raise ValueError("Result contains tests outside the requested namespace, fixture, or test")
    return {"total": total, "passed": passed, "failed": failed, "skipped": skipped}


def command(editor, project, evidence, platform, test_filter, graphics):
    argv = [str(editor), "-batchmode"]
    if not graphics:
        argv.append("-nographics")
    # -quit can terminate a Test Framework run before it has produced its XML.
    return argv + ["-projectPath", str(project), "-runTests", "-testPlatform", platform,
                   "-testFilter", test_filter, "-testResults", str(evidence / "results.xml"),
                   "-logFile", str(evidence / "editor.log")]


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--platform", choices=["editmode", "playmode"], required=True)
    parser.add_argument("--filter", required=True, help="Fully qualified namespace, fixture, or test name (letters, digits, dots, underscores)")
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=240)
    parser.add_argument("--graphics", action="store_true", help="Use the normal local display instead of -nographics")
    args = parser.parse_args(argv)
    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_.]*", args.filter) or args.timeout <= 0:
        parser.error("Provide a literal test name and a positive timeout")
    evidence = unity.new_evidence("tests-" + args.platform)
    project = evidence / "project"
    save_path = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        hashes = unity.snapshot(unity.ROOT, project)
        unity.record_source(evidence, hashes)
        save_path = unity.isolate_save_identity(project)
        argv = command(editor, project, evidence, args.platform, args.filter, args.graphics)
        code = unity.execute(argv, evidence, args.timeout, "test")
        if code:
            raise ValueError(f"Unity test process failed with exit {code}")
        unity.validate_log(evidence / "editor.log")
        result = validate_results(evidence / "results.xml", args.filter)
        unity.receipt(evidence, status="passed", editor_version=version, command=argv,
                      results=result, disposable_save_path=str(save_path), graphics=args.graphics)
        print(f"PASS: {result['passed']} {args.platform} tests. Evidence: {evidence}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error), disposable_save_path=str(save_path) if save_path else None)
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)
        if save_path is not None and save_path.exists():
            shutil.rmtree(save_path)


if __name__ == "__main__":
    raise SystemExit(main())
