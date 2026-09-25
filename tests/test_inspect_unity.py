import copy
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from scripts import inspect_unity


class UnityInventory(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.home = Path(self.temp.name)
        self.editor = self.home / "custom/Editor/Unity"
        self.editor.parent.mkdir(parents=True)
        self.editor.touch()
        self.editor.chmod(0o700)
        (self.editor.parent / "Data/PlaybackEngines/LinuxStandaloneSupport").mkdir(parents=True)

    def inventory(self, found=True):
        def probe(args):
            if args[-1] == "-version":
                return (0, "6000.4.10f1\n") if found else (1, "private diagnostics must not be copied")
            return 0, '00:00.0 "VGA compatible controller" "Example" "GPU"'
        with patch.object(inspect_unity, "probe", side_effect=probe):
            return inspect_unity.inspect(self.home, str(self.editor))

    def test_explicit_editor_is_verified_without_certifying_license(self):
        report = self.inventory()
        inspect_unity.validate(report)
        self.assertEqual(report['editor']['version'], '6000.4.10f1')
        self.assertEqual(report['editor']['build_support'], ['LinuxStandaloneSupport'])
        self.assertEqual(report['readiness']['license'], 'unverified')

    def test_failed_version_is_explicitly_blocked_and_redacted(self):
        report = self.inventory(False)
        inspect_unity.validate(report)
        self.assertIsNone(report['editor'])
        self.assertEqual(report['readiness']['state'], 'blocked')
        self.assertNotIn('private diagnostics', str(report))

    def test_standard_hub_location_is_discovered(self):
        path = self.home / 'Unity/Hub/Editor/6000.4.10f1/Editor/Unity'
        path.parent.mkdir(parents=True)
        path.touch()
        self.assertIn(path, inspect_unity.candidates(self.home))

    def test_missing_editor_is_valid_only_with_explicit_blocker(self):
        report = self.inventory(False)
        report['readiness']['blockers'] = []
        with self.assertRaises(ValueError):
            inspect_unity.validate(report)

    def test_provenance_and_success_claims_are_validated(self):
        base = self.inventory()
        for change in [lambda x: x.pop('provenance'),
                       lambda x: x['readiness'].update(license='valid'),
                       lambda x: x['readiness'].update(state='ready'),
                       lambda x: x['editor'].update(version='invented'),
                       lambda x: x.update(memory_bytes=None),
                       lambda x: x.update(observed_at='yesterday')]:
            report = copy.deepcopy(base)
            change(report)
            with self.subTest(report=report), self.assertRaises(ValueError):
                inspect_unity.validate(report)


if __name__ == '__main__':
    unittest.main()
