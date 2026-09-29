from pathlib import Path
import tempfile
import unittest
from unittest import mock

from scripts import import_mixamo


class MixamoImport(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "repo"
        self.downloads = Path(self.temp.name) / "downloads"
        self.downloads.mkdir()
        patcher = mock.patch.object(import_mixamo, "ROOT", self.root)
        patcher.start()
        self.addCleanup(patcher.stop)

    def test_slug(self):
        self.assertEqual(import_mixamo.slug("Breathing Idle (1).fbx"), "breathing-idle-1")
        self.assertEqual(import_mixamo.slug("Talking.fbx"), "talking")

    def test_imports_binary_fbx_with_licensed_entry(self):
        (self.downloads / "Breathing Idle.fbx").write_bytes(import_mixamo.FBX_BINARY + b"  data")
        entries = import_mixamo.import_folder(self.downloads)
        copied = self.root / import_mixamo.DESTINATION / "breathing-idle.fbx"
        self.assertTrue(copied.is_file())
        self.assertEqual(entries[0]["kind"], "licensed")
        self.assertEqual(entries[0]["source"], "Mixamo")
        self.assertEqual(entries[0]["license"], "Adobe-Mixamo-Terms")
        self.assertTrue(entries[0]["path"].startswith(import_mixamo.DESTINATION + "/"))

    def test_rejects_non_binary_fbx_and_empty_folder(self):
        with self.assertRaises(ValueError):
            import_mixamo.import_folder(self.downloads)
        (self.downloads / "idle.fbx").write_bytes(b"; FBX 7.4.0 project file")
        with self.assertRaises(ValueError):
            import_mixamo.import_folder(self.downloads)


if __name__ == "__main__":
    unittest.main()
