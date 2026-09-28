import hashlib
from pathlib import Path
import tempfile
import unittest
from unittest import mock

from scripts import characters


class CharacterBuildProvenance(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        patcher = mock.patch.object(characters, "ROOT", self.root)
        patcher.start()
        self.addCleanup(patcher.stop)

    def write(self, relative, data):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        return path

    def test_entries_record_generated_fbx_and_cc0_textures(self):
        fbx = f"{characters.CHARACTERS}/a/a.fbx"
        self.write(fbx, b"fbx")
        self.write(f"{characters.CHARACTERS}/Textures/skins/s/s.png", b"skin")
        description = {"textures": [{"path": "skins/s/s.png", "authors": ["Someone"]}]}
        entries = characters.provenance_entries("a", description, fbx, {"skins/s/s.png": "skins01"})
        self.assertEqual(entries[0]["kind"], "generated")
        self.assertEqual(entries[0]["generator"], characters.BUILD_SCRIPT)
        self.assertEqual(entries[0]["sha256"], hashlib.sha256(b"fbx").hexdigest())
        texture = entries[1]
        self.assertEqual(texture["source"], "MakeHuman")
        self.assertEqual(texture["url"], characters.PACK_PAGE.format("skins01"))
        self.assertEqual(texture["retrieved"], characters.PACKS["skins01"][1])
        self.assertEqual(texture["authors"], ["Someone"])
        self.assertEqual(texture["sha256"], hashlib.sha256(b"skin").hexdigest())

    def test_texture_outside_pinned_packs_fails(self):
        fbx = f"{characters.CHARACTERS}/a/a.fbx"
        self.write(fbx, b"fbx")
        with self.assertRaises(ValueError):
            characters.provenance_entries("a", {"textures": [{"path": "hair/x/x.png", "authors": ["A"]}]}, fbx, {})


class CharacterDownloads(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        patcher = mock.patch.object(characters, "CACHE", Path(self.temp.name) / "cache")
        patcher.start()
        self.addCleanup(patcher.stop)
        self.source = Path(self.temp.name) / "file.bin"
        self.source.write_bytes(b"payload")

    def test_cached_download_checks_sha256(self):
        digest = hashlib.sha256(b"payload").hexdigest()
        path = characters.cached("file.bin", [self.source.as_uri()], digest)
        self.assertEqual(path.read_bytes(), b"payload")
        # A second call reuses the verified cache without downloading.
        self.assertEqual(characters.cached("file.bin", ["file:///nonexistent"], digest), path)

    def test_mismatched_download_is_rejected_and_removed(self):
        with self.assertRaises(ValueError):
            characters.cached("file.bin", [self.source.as_uri()], "0" * 64)
        self.assertFalse((characters.CACHE / "file.bin").exists())
        self.assertFalse((characters.CACHE / "file.bin.part").exists())

    def test_pack_urls_use_both_mirrors(self):
        urls = characters.pack_urls("skins01")
        self.assertEqual(len(urls), 2)
        self.assertTrue(all(url.endswith("/skins01/skins01_cc0.zip") for url in urls))


if __name__ == "__main__":
    unittest.main()
