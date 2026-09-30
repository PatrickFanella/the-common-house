import tempfile
from pathlib import Path
import unittest
from scripts.build_season_assets import KIT, SCENE, copy_export


class NarrowExportTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.base = Path(self.temp.name)
        self.export = self.base / "export"
        self.root = self.base / "root"
        self.export.mkdir()
        self.root.mkdir()

    def complete(self):
        for i in range(16):
            path = self.export / KIT / "Prefabs" / f"{i}.prefab"
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("prefab")
        scene = self.export / SCENE
        scene.parent.mkdir(parents=True, exist_ok=True)
        scene.write_text("review scene")

    def test_narrow_export_preserves_owner_settings(self):
        self.complete()
        owner = self.root / "ProjectSettings/GraphicsSettings.asset"
        owner.parent.mkdir()
        owner.write_text("owner edit")
        self.assertEqual(copy_export(self.export, self.root), 17)
        self.assertEqual(owner.read_text(), "owner edit")
        self.assertEqual((self.root / SCENE).read_text(), "review scene\n")

    def test_unexpected_file_rejects_entire_export_before_copy(self):
        self.complete()
        rogue = self.export / "ProjectSettings/GraphicsSettings.asset"
        rogue.parent.mkdir()
        rogue.write_text("unexpected")
        with self.assertRaisesRegex(ValueError, "Unexpected exported file"):
            copy_export(self.export, self.root)
        self.assertFalse((self.root / SCENE).exists())

    def test_partial_export_cannot_be_reported_as_success(self):
        self.complete()
        (self.export / KIT / "Prefabs/0.prefab").unlink()
        with self.assertRaisesRegex(ValueError, "Incomplete"):
            copy_export(self.export, self.root)
        self.assertFalse((self.root / SCENE).exists())

    def test_symlink_cannot_export_outside_data(self):
        self.complete()
        outside = self.base / "outside"
        outside.write_text("private")
        (self.export / KIT / "Prefabs/extra.prefab").symlink_to(outside)
        with self.assertRaisesRegex(ValueError, "Unexpected exported file"):
            copy_export(self.export, self.root)
        self.assertFalse((self.root / SCENE).exists())


if __name__ == "__main__":
    unittest.main()
