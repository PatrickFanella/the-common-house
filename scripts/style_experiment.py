#!/usr/bin/env python3
"""Build or capture one of the round-two style experiments (issues #71-#73).

    python3 scripts/style_experiment.py build prerendered
    python3 scripts/style_experiment.py capture prerendered

`build` runs the experiment's Editor Build method in a disposable copy and copies the result
back; `capture` runs its Capture method in a disposable copy and checks that every image exists.
These experiments reuse the N64 experiment's models and face photo, so run
scripts/build_n64.py first when those change.
"""
import argparse
from pathlib import Path
import shutil
import subprocess
import sys

try:
    from . import build_n64
    from . import unity_common as unity
except ImportError:
    import build_n64
    import unity_common as unity

ROOT = Path(__file__).resolve().parents[1]
EXPERIMENTS = {
    "prerendered": {
        "class": "CommonHouse.Editor.Experiments.PrerenderedOutpost",
        "folder": "Assets/CommonHouse/Experiments/Prerendered",
        "images": ["radio-room", "kitchen-low", "kitchen-high", "archive", "workshop"],
    },
    "isometric": {
        "class": "CommonHouse.Editor.Experiments.IsometricOutpost",
        "folder": "Assets/CommonHouse/Experiments/Isometric",
        "images": ["overview", "radio-room", "kitchen", "archive"],
    },
    "anime": {
        "class": "CommonHouse.Editor.Experiments.AnimeOutpost",
        "folder": "Assets/CommonHouse/Experiments/Anime",
        "images": ["corridor", "resident", "radio-desk", "kitchen", "archive-shelf", "workshop"],
    },
}


def build(name, experiment):
    folder = ROOT / experiment["folder"]
    if folder.exists():
        shutil.rmtree(folder)
    meta = folder.with_name(folder.name + ".meta")
    if meta.exists():
        meta.unlink()
    code = subprocess.run([sys.executable, str(ROOT / "scripts/run_editor_method.py"),
                           experiment["class"] + ".Build", "--copy-back"]).returncode
    if code:
        return code
    subprocess.run(["git", "-C", str(ROOT), "checkout", "--", "ProjectSettings/ShaderGraphSettings.asset"], check=False)
    build_n64.strip_yaml_whitespace()
    print(f"PASS: {name} experiment built")
    return 0


def capture(name, experiment, timeout):
    evidence = unity.new_evidence(name)
    project = evidence / "project"
    save = None
    try:
        editor, version = unity.resolve_editor()
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        save = unity.isolate_save_identity(project)
        command = [str(editor), "-batchmode", "-quit", "-projectPath", str(project),
                   "-executeMethod", experiment["class"] + ".Capture", "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, timeout, "test")
        if code:
            raise ValueError(f"Graphical Editor run failed with exit {code}")
        unity.validate_log(evidence / "editor.log", require_import=True)
        required = [f"{image}.png" for image in experiment["images"]]
        for image in required:
            path = evidence / "test-data" / image
            if not path.is_file() or path.stat().st_size == 0:
                raise ValueError(f"Missing {name} image: {image}")
        unity.receipt(evidence, status="passed", editor_version=version, command=command, artifacts=required,
                      disposable_save_path=str(save))
        print(f"PASS: {name} renders available for inspection at {evidence / 'test-data'}")
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


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("action", choices=["build", "capture"])
    parser.add_argument("experiment", choices=sorted(EXPERIMENTS))
    parser.add_argument("--timeout", type=int, default=900)
    args = parser.parse_args(argv)
    experiment = EXPERIMENTS[args.experiment]
    if args.action == "build":
        return build(args.experiment, experiment)
    return capture(args.experiment, experiment, args.timeout)


if __name__ == "__main__":
    raise SystemExit(main())
