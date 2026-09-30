#!/usr/bin/env python3
"""Capture actual Game-view panels in an isolated graphical Editor."""
import argparse
import json
import shutil
import sys

try:
    from . import unity_common as unity
except ImportError:
    import unity_common as unity


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--editor")
    selection = parser.add_mutually_exclusive_group()
    selection.add_argument("--guidance", action="store_true", help="Capture only the welcome and plain-pause controls.")
    selection.add_argument("--intro", action="store_true", help="Capture the isolated arrival opening and its readable actions.")
    parser.add_argument("--scale", type=float, choices=(1.0, 1.5), default=1.0)
    parser.add_argument("--timeout", type=int, default=240)
    args = parser.parse_args(argv)
    evidence = unity.new_evidence("panel-graphics")
    project = evidence / "project"
    save = None
    try:
        editor, version = unity.resolve_editor(explicit=args.editor)
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        save = unity.isolate_save_identity(project)
        (evidence / "test-data").mkdir()
        (evidence / "test-data/capture-settings.json").write_text(json.dumps({"width": 800, "height": 600, "scale": args.scale, "guidance": args.guidance}))
        command = [str(editor), "-projectPath", str(project), "-executeMethod",
                   "CommonHouse.Editor.IntroOpeningEvidence.Capture" if args.intro else "CommonHouse.Editor.PanelEvidence.Capture",
                   "-logFile", str(evidence / "editor.log")]
        code = unity.execute(command, evidence, args.timeout, "test")
        if code:
            raise ValueError(f"Graphical Editor failed with exit {code}")
        unity.validate_log(evidence / "editor.log")
        required = ["inspection-panel.png", "transmission-panel.png", "notebook-panel.png", "dialogue-panel.png", "opening-panel.png", "archive-panel.png", "comparison-panel.png", "routing-panel.png", "missing-page-panel.png", "roster-comparison-panel.png", "repair-account-panel.png", "consent-panel.png", "send-private-panel.png", "aftermath-private-panel.png", "send-refusal-panel.png", "aftermath-refusal-panel.png", "send-public-panel.png", "aftermath-public-panel.png", "hint-panel.png", "comparison-scrolled-panel.png", "panel-evidence.txt"]
        if args.guidance:
            required = ["welcome-controls.png", "pause-controls.png", "panel-evidence.txt"]
        if args.intro:
            required = ["arrival-world.png", "arrival-choice.png", "return-warning.png", "warning-actions.png", "welcome.png", "welcome-actions.png", "arrival-pause.png", "radio-programs.png", "radio-actions.png", "office-greeting.png", "greeting-actions.png", "orientation.png", "orientation-actions.png", "closing-choice.png", "closing-actions.png", "arrival-complete.png", "intro-evidence.txt"]
        for name in required:
            path = evidence / "test-data" / name
            if not path.is_file() or not path.stat().st_size:
                raise ValueError(f"Missing artifact: {name}")
        unity.receipt(evidence, status="passed", editor_version=version, command=command,
                      artifacts=required, resolution=[800, 600], text_scale=args.scale, guidance_only=args.guidance,
                      intro_only=args.intro, disposable_save_path=str(save))
        print(f"PASS: rendered panels available for inspection at {evidence}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error))
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)
        if save is not None and save.exists():
            shutil.rmtree(save)


if __name__ == "__main__":
    raise SystemExit(main())
