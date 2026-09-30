import contextlib
import io
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from scripts import capture_panels


class CapturePanelTests(unittest.TestCase):
    def run_capture(self, arguments, artifacts):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            evidence = root / "evidence"
            evidence.mkdir()
            save = root / "save"
            save.mkdir()

            def snapshot(source, destination):
                destination.mkdir()
                return {}

            def execute(command, output, timeout, mode):
                self.assertEqual(mode, "test")
                self.assertNotIn("-quit", command)
                for name in artifacts:
                    (output / "test-data" / name).write_bytes(b"test artifact")
                return 0

            with patch.object(capture_panels.unity, "new_evidence", return_value=evidence), \
                 patch.object(capture_panels.unity, "resolve_editor", return_value=(Path("/test/Unity"), "6000.4.10f1")), \
                 patch.object(capture_panels.unity, "snapshot", side_effect=snapshot), \
                 patch.object(capture_panels.unity, "record_source"), \
                 patch.object(capture_panels.unity, "isolate_save_identity", return_value=save), \
                 patch.object(capture_panels.unity, "execute", side_effect=execute) as invocation, \
                 patch.object(capture_panels.unity, "validate_log"), \
                 patch.object(capture_panels.unity, "receipt") as receipt, \
                 contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                result = capture_panels.main(arguments)
                self.assertFalse((evidence / "project").exists())
                self.assertFalse(save.exists())
                return result, invocation.call_args.args[0], receipt.call_args.kwargs

    def test_intro_selects_its_editor_method_and_requires_all_rendered_artifacts(self):
        names = ["arrival-world.png", "arrival-choice.png", "return-warning.png", "warning-actions.png",
                 "welcome.png", "welcome-actions.png", "arrival-pause.png", "intro-evidence.txt"]
        result, command, receipt = self.run_capture(["--intro", "--scale", "1.5"], names)
        self.assertEqual(result, 0)
        self.assertIn("CommonHouse.Editor.IntroOpeningEvidence.Capture", command)
        self.assertEqual(receipt["artifacts"], names)
        self.assertEqual(receipt["text_scale"], 1.5)
        self.assertTrue(receipt["intro_only"])
        self.assertEqual(receipt["status"], "passed")

    def test_missing_intro_capture_fails_and_cleans_disposable_state(self):
        result, _, receipt = self.run_capture(["--intro"], ["arrival-world.png"])
        self.assertEqual(result, 1)
        self.assertEqual(receipt["status"], "failed")
        self.assertIn("arrival-choice.png", receipt["error"])

    def test_existing_guidance_capture_keeps_its_method_and_artifact_contract(self):
        names = ["welcome-controls.png", "pause-controls.png", "panel-evidence.txt"]
        result, command, receipt = self.run_capture(["--guidance"], names)
        self.assertEqual(result, 0)
        self.assertIn("CommonHouse.Editor.PanelEvidence.Capture", command)
        self.assertEqual(receipt["artifacts"], names)
        self.assertFalse(receipt["intro_only"])

    def test_intro_and_baseline_guidance_cannot_be_requested_together(self):
        with contextlib.redirect_stderr(io.StringIO()), self.assertRaises(SystemExit) as error:
            capture_panels.main(["--intro", "--guidance"])
        self.assertEqual(error.exception.code, 2)


if __name__ == "__main__":
    unittest.main()
