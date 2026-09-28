#!/usr/bin/env python3
"""Render each generated character for review in a disposable project copy."""
import argparse
import shutil
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity

RECIPES = unity.ROOT / "tools/characters/recipes"
VIEWS = ["front", "three-quarter", "face"]


def artifacts():
    names = ["character-inventory.txt", "graphics-device.txt"]
    for recipe in sorted(RECIPES.glob("*.json")):
        names += [f"{recipe.stem}-{view}.jpg" for view in VIEWS]
    return names


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--editor")
    parser.add_argument("--timeout", type=int, default=600)
    args = parser.parse_args(argv)
    evidence = unity.new_evidence("characters")
    project = evidence / "project"
    save = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        save = unity.isolate_save_identity(project)
        command = [str(editor), "-batchmode", "-quit", "-projectPath", str(project),
                   "-executeMethod", "CommonHouse.Editor.CharacterStudio.Capture",
                   "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, args.timeout, "test")
        if code:
            raise ValueError(f"Graphical Editor run failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)
        required = artifacts()
        for name in required:
            path = evidence / "test-data" / name
            if not path.is_file() or path.stat().st_size == 0:
                raise ValueError(f"Missing character artifact: {name}")
        unity.receipt(evidence, status="passed", editor_version=version, command=command,
                      artifacts=required, disposable_save_path=str(save))
        print(f"PASS: character renders available for inspection at {evidence / 'test-data'}")
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
