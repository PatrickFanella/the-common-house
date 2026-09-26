from pathlib import Path
import subprocess
import tempfile
import unittest

from scripts import check_binary_sizes


class BinarySizeChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def write(self, name, data):
        path = self.root / name
        path.write_bytes(data)
        return path

    def test_small_binary_passes(self):
        path = self.write("small.png", b"\x89PNG\0" + b"x" * 10)
        self.assertEqual(check_binary_sizes.check([path], self.root, limit=100), (1, []))

    def test_oversized_binary_fails_with_name(self):
        path = self.write("large.png", b"\0" * 101)
        binaries, errors = check_binary_sizes.check([path], self.root, limit=100)
        self.assertEqual(binaries, 1)
        self.assertIn("large.png", errors[0])

    def test_large_text_is_not_a_binary(self):
        path = self.write("scene.unity", b"a: 1\n" * 100)
        self.assertEqual(check_binary_sizes.check([path], self.root, limit=100), (0, []))

    def test_missing_and_symlinked_paths_are_skipped(self):
        target = self.write("target.bin", b"\0" * 200)
        link = self.root / "link.bin"
        link.symlink_to(target)
        self.assertEqual(check_binary_sizes.check([link, self.root / "gone.bin"], self.root, limit=100), (0, []))

    def test_tracked_includes_staged_files(self):
        subprocess.run(["git", "init", "-q", str(self.root)], check=True)
        self.write("staged.bin", b"\0")
        subprocess.run(["git", "-C", str(self.root), "add", "staged.bin"], check=True)
        self.assertEqual([path.name for path in check_binary_sizes.tracked(self.root)], ["staged.bin"])

    def test_lfs_paths_skip_the_cap(self):
        path = self.write("large.png", b"\0" * 101)
        self.assertEqual(check_binary_sizes.check([path], self.root, limit=100, lfs={path}), (0, []))

    def test_lfs_path_without_pointer_fails(self):
        path = self.write("raw.png", b"\0" * 10)
        _, errors = check_binary_sizes.check([path], self.root, limit=100, lfs={path}, is_pointer=lambda p: False)
        self.assertIn("raw.png", errors[0])
        self.assertIn("LFS pointer", errors[0])

    def test_attributes_and_staged_pointers_are_read_from_git(self):
        # Stage blobs before .gitattributes exists so the test does not need git-lfs installed.
        git = ["git", "-C", str(self.root)]
        subprocess.run(["git", "init", "-q", str(self.root)], check=True)
        raw = self.write("raw.png", b"\0" * 10)
        pointer = self.write("pointer.png", check_binary_sizes.POINTER_PREFIX + b"oid sha256:" + b"0" * 64 + b"\nsize 10\n")
        plain = self.write("plain.bin", b"\0")
        subprocess.run(git + ["add", "raw.png", "pointer.png", "plain.bin"], check=True)
        self.write(".gitattributes", b"*.png filter=lfs diff=lfs merge=lfs -text\n")
        paths = check_binary_sizes.tracked(self.root)
        self.assertEqual(check_binary_sizes.lfs_tracked(self.root, paths), {raw, pointer})
        self.assertFalse(check_binary_sizes.staged_blob_is_pointer(self.root, raw))
        self.assertTrue(check_binary_sizes.staged_blob_is_pointer(self.root, pointer))
        self.assertNotIn(plain, check_binary_sizes.lfs_tracked(self.root, paths))


if __name__ == "__main__":
    unittest.main()
