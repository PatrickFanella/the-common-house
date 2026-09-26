import hashlib
from pathlib import Path
import tempfile
import unittest

from scripts import check_binary_sizes, check_provenance

SOURCED = "Assets/CommonHouse/Art/Sourced/PolyHaven/planks/"


class ProvenanceChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def write(self, name, data):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        return path

    def cc0(self, name, data, **changes):
        entry = {
            "path": name, "kind": "cc0", "sha256": hashlib.sha256(data).hexdigest(),
            "source": "Poly Haven", "url": "https://polyhaven.com/a/planks",
            "authors": ["Example Author"], "license": "CC0-1.0", "retrieved": "2026-09-26",
        }
        entry.update(changes)
        return entry

    def run_check(self, entries, tracked, lfs=()):
        return check_provenance.check({"version": 1, "assets": entries}, self.root, tracked, set(lfs))

    def test_empty_manifest_passes_without_sourced_files(self):
        self.assertEqual(self.run_check([], []), (0, []))

    def test_documented_cc0_file_passes(self):
        data = b"\0texture"
        path = self.write(SOURCED + "diff.png", data)
        meta = self.write(SOURCED + "diff.png.meta", b"guid: 1\n")
        self.assertEqual(self.run_check([self.cc0(SOURCED + "diff.png", data)], [path, meta], [path]), (1, []))

    def test_sourced_file_without_entry_fails(self):
        path = self.write(SOURCED + "diff.png", b"x")
        _, errors = self.run_check([], [path])
        self.assertIn("no provenance entry", errors[0])

    def test_lfs_file_outside_sourced_root_needs_entry(self):
        path = self.write("Assets/CommonHouse/Art/Baked/ao.png", b"x")
        _, errors = self.run_check([], [path], [path])
        self.assertIn("Baked/ao.png", errors[0])

    def test_generated_entry_needs_existing_generator(self):
        data = b"baked"
        path = self.write("Assets/CommonHouse/Art/Baked/ao.png", data)
        generator = self.write("Assets/Editor/Bake.cs", b"class Bake {}")
        entry = {"path": "Assets/CommonHouse/Art/Baked/ao.png", "kind": "generated",
                 "sha256": hashlib.sha256(data).hexdigest(), "generator": "Assets/Editor/Bake.cs"}
        self.assertEqual(self.run_check([entry], [path, generator], [path]), (1, []))
        entry["generator"] = "Assets/Editor/Missing.cs"
        self.assertIn("generator", self.run_check([entry], [path], [path])[1][0])

    def test_rejects_license_source_url_and_date_problems(self):
        data = b"x"
        path = self.write(SOURCED + "diff.png", data)
        cases = {
            "license": self.cc0(SOURCED + "diff.png", data, license="CC-BY-4.0"),
            "source": self.cc0(SOURCED + "diff.png", data, source="Somewhere"),
            "url": self.cc0(SOURCED + "diff.png", data, url="https://polyhaven.com.example.net/a/planks"),
            "retrieved": self.cc0(SOURCED + "diff.png", data, retrieved="yesterday"),
            "authors": self.cc0(SOURCED + "diff.png", data, authors=[]),
        }
        for field, entry in cases.items():
            with self.subTest(field=field):
                _, errors = self.run_check([entry], [path])
                self.assertTrue(any(field in error for error in errors), errors)

    def test_hash_mismatch_fails(self):
        path = self.write(SOURCED + "diff.png", b"new")
        _, errors = self.run_check([self.cc0(SOURCED + "diff.png", b"old")], [path])
        self.assertIn("sha256 does not match", errors[0])

    def test_unfetched_lfs_pointer_uses_its_oid(self):
        data = b"real texture"
        oid = hashlib.sha256(data).hexdigest()
        pointer = check_binary_sizes.POINTER_PREFIX + f"oid sha256:{oid}\nsize {len(data)}\n".encode()
        path = self.write(SOURCED + "diff.png", pointer)
        self.assertEqual(self.run_check([self.cc0(SOURCED + "diff.png", data)], [path], [path]), (1, []))

    def test_entry_for_untracked_or_duplicate_path_fails(self):
        data = b"x"
        path = self.write(SOURCED + "diff.png", data)
        entry = self.cc0(SOURCED + "diff.png", data)
        self.assertIn("not tracked", self.run_check([entry], [])[1][0])
        self.assertIn("duplicate", self.run_check([entry, dict(entry)], [path])[1][0])

    def test_unknown_fields_and_bad_manifest_fail(self):
        data = b"x"
        path = self.write(SOURCED + "diff.png", data)
        self.assertIn("unknown fields", self.run_check([self.cc0(SOURCED + "diff.png", data, note="x")], [path])[1][0])
        self.assertIn("manifest", check_provenance.check({"assets": []}, self.root, [], set())[1][0])


if __name__ == "__main__":
    unittest.main()
