from pathlib import Path
import json
import tempfile
import unittest
from unittest.mock import patch

from scripts import unity_common as unity


class UnityImportGuards(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / 'source'
        self.root.mkdir()
        for name in ['Assets', 'Packages', 'ProjectSettings']:
            (self.root / name).mkdir()
        (self.root / 'Packages/manifest.json').write_text('{"dependencies": {}}')
        (self.root / 'ProjectSettings/ProjectVersion.txt').write_text('m_EditorVersion: 6000.4.10f1\n')
        self.destination = self.root.parent / 'snapshot'

    def test_snapshot_copies_source_without_caches_or_userdata(self):
        (self.root / 'Assets/a.txt').write_text('source')
        (self.root / 'Library').mkdir()
        (self.root / 'Library/user-save').write_text('private')
        hashes = unity.snapshot(self.root, self.destination)
        self.assertEqual((self.destination / 'Assets/a.txt').read_text(), 'source')
        self.assertIn('Assets/a.txt', hashes)
        self.assertFalse((self.destination / 'Library').exists())

    def test_symlink_cannot_escape(self):
        (self.root / 'Assets/escape').symlink_to(self.root.parent)
        with self.assertRaises(ValueError):
            unity.snapshot(self.root, self.destination)

    def test_local_package_access_is_rejected(self):
        (self.root / 'Packages/manifest.json').write_text(json.dumps({'dependencies': {'local': 'file:/private'}}))
        with self.assertRaises(ValueError):
            unity.snapshot(self.root, self.destination)

    def test_mismatched_version_is_rejected(self):
        editor = self.root.parent / 'Unity'
        editor.touch(); editor.chmod(0o700)
        with patch.object(unity.subprocess, 'run') as run:
            run.return_value.returncode = 0
            run.return_value.stdout = '6000.1.0f1'
            with self.assertRaisesRegex(ValueError, 'does not match'):
                unity.resolve_editor(self.root, str(editor))

    def test_compile_error_fails_even_with_completion_marker(self):
        log = self.root.parent / 'editor.log'
        log.write_text('error CS1002: missing semicolon\nExiting batchmode successfully now!')
        with self.assertRaises(ValueError):
            unity.validate_log(log, require_import=True)

    def test_missing_or_incomplete_log_fails(self):
        log = self.root.parent / 'editor.log'
        with self.assertRaises(ValueError):
            unity.validate_log(log, require_import=True)
        log.write_text('Starting editor...')
        with self.assertRaises(ValueError):
            unity.validate_log(log, require_import=True)

    def test_save_namespace_is_unique_and_changes_only_the_copy(self):
        original = '  companyName: CommonHouseDevelopment\n  productName: The Common House POC\n'
        (self.root / 'ProjectSettings/ProjectSettings.asset').write_text(original)
        unity.snapshot(self.root, self.destination)
        path = unity.isolate_save_identity(self.destination)
        self.assertEqual((self.root / 'ProjectSettings/ProjectSettings.asset').read_text(), original)
        self.assertEqual(path.parent.name, 'CommonHouseVerification')
        self.assertTrue(path.name.startswith('run-'))
        self.assertFalse(path.exists())


if __name__ == '__main__':
    unittest.main()
