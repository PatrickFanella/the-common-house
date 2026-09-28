import hashlib
import json
from pathlib import Path
import tempfile
import unittest
from unittest import mock

from scripts import fetch_polyhaven


def listing(key, name, resolution="2k", fmt="jpg"):
    return {key: {resolution: {fmt: {"url": f"https://dl.polyhaven.org/file/{name}", "md5": "m-" + name}}}}


class PolyHavenPlanning(unittest.TestCase):
    def test_texture_plans_three_jpg_maps(self):
        files = {**listing("Diffuse", "a_diff_2k.jpg"), **listing("nor_gl", "a_nor_gl_2k.jpg"),
                 **listing("arm", "a_arm_2k.jpg"), **listing("nor_dx", "a_nor_dx_2k.jpg"), **listing("Rough", "a_rough_2k.jpg")}
        names = [name for name, _, _ in fetch_polyhaven.plan(files, 0, "2k")]
        self.assertEqual(sorted(names), ["a_arm_2k.jpg", "a_diff_2k.jpg", "a_nor_gl_2k.jpg"])

    def test_model_adds_part_maps_and_fbx(self):
        files = {**listing("Diffuse", "m_diff_2k.jpg"), **listing("nor_gl", "m_nor_gl_2k.jpg"), **listing("arm", "m_arm_2k.jpg"),
                 **listing("glass_diff", "m_glass_diff_2k.jpg"), **listing("glass_arm", "m_glass_arm_2k.jpg"),
                 **listing("glass_nor_gl", "m_glass_nor_gl_2k.jpg"), **listing("fbx", "m_2k.fbx", fmt="fbx")}
        names = {name for name, _, _ in fetch_polyhaven.plan(files, fetch_polyhaven.MODEL_TYPE, "2k")}
        self.assertIn("m_2k.fbx", names)
        self.assertIn("m_glass_nor_gl_2k.jpg", names)
        self.assertEqual(len(names), 7)

    def test_missing_resolution_fails(self):
        with self.assertRaises(ValueError):
            fetch_polyhaven.plan(listing("Diffuse", "a_diff_1k.jpg", "1k"), 0, "2k")

    def test_merge_replaces_same_path_and_sorts(self):
        manifest = {"version": 1, "assets": [{"path": "b", "sha256": "old"}, {"path": "c"}]}
        fetch_polyhaven.merge(manifest, [{"path": "b", "sha256": "new"}, {"path": "a"}])
        self.assertEqual([item["path"] for item in manifest["assets"]], ["a", "b", "c"])
        self.assertEqual(manifest["assets"][1]["sha256"], "new")


class PolyHavenDownload(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        patcher = mock.patch.object(fetch_polyhaven, "ROOT", Path(self.temp.name))
        patcher.start()
        self.addCleanup(patcher.stop)

    def fake_get(self, content, md5=None):
        data = content
        files = {"Diffuse": {"1k": {"jpg": {"url": "https://dl/x_diff_1k.jpg", "md5": md5 or hashlib.md5(data).hexdigest()}}}}
        responses = {
            "https://api.polyhaven.com/info/x": json.dumps({"type": 0, "authors": {"B Author": "All", "A Author": "Photo"}}).encode(),
            "https://api.polyhaven.com/files/x": json.dumps(files).encode(),
            "https://dl/x_diff_1k.jpg": data,
        }
        return responses.__getitem__

    def test_download_writes_file_and_provenance(self):
        entries = fetch_polyhaven.download("x", "1k", "2026-09-28", get=self.fake_get(b"pixels"))
        path = Path(self.temp.name) / fetch_polyhaven.DESTINATION / "x" / "x_diff_1k.jpg"
        self.assertEqual(path.read_bytes(), b"pixels")
        self.assertEqual(entries[0]["authors"], ["A Author", "B Author"])
        self.assertEqual(entries[0]["url"], "https://polyhaven.com/a/x")
        self.assertEqual(entries[0]["sha256"], hashlib.sha256(b"pixels").hexdigest())

    def test_md5_mismatch_fails_without_entry(self):
        with self.assertRaises(ValueError):
            fetch_polyhaven.download("x", "1k", "2026-09-28", get=self.fake_get(b"pixels", md5="0" * 32))


if __name__ == "__main__":
    unittest.main()
