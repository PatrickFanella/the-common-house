import contextlib
import io
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from scripts import check_docs


class DocumentChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "repo"
        self.root.mkdir()
        self.readme = self.root / "README.md"

    def check(self, text):
        self.readme.write_text(text, encoding="utf-8")
        return check_docs.check(self.root, [self.readme])

    def test_valid_relative_percent_encoded_and_angle_links(self):
        (self.root / "docs").mkdir()
        (self.root / "docs/a b.md").write_text("Content")
        links, errors = self.check('[one](docs/a%20b.md#heading) [two](<docs/a b.md>)')
        self.assertEqual((links, errors), (2, []))

    def test_missing_target_is_failure(self):
        self.assertIn("missing file", self.check('[missing](absent.md)')[1][0])

    def test_empty_document_is_failure(self):
        self.assertIn("empty document", self.check(' \n')[1][0])

    def test_traversal_and_encoded_traversal_are_failures(self):
        outside = self.root.parent / "private.md"
        outside.write_text("private")
        for link in ['../private.md', '%2e%2e/private.md', str(outside)]:
            with self.subTest(link=link):
                self.assertIn("escapes repository", self.check(f'[x]({link})')[1][0])

    def test_external_symlink_is_failure(self):
        outside = self.root.parent / "private.md"
        outside.write_text("private")
        (self.root / "linked.md").symlink_to(outside)
        self.assertIn("escapes repository", self.check('[x](linked.md)')[1][0])

    def test_directory_is_not_a_file_target(self):
        (self.root / "docs").mkdir()
        self.assertIn("missing file", self.check('[x](docs/)')[1][0])

    def test_external_urls_and_anchors_do_not_fetch(self):
        links, errors = self.check('[a](https://invalid.example/a) [b](mailto:x@example.invalid) [c](#local) [d](//invalid.example/a)')
        self.assertEqual((links, errors), (0, []))

    def test_code_examples_are_not_checked(self):
        self.assertEqual(self.check('```md\n[x](missing.md)\n```\n`[y](missing.md)`'), (0, []))

    def test_images_are_checked(self):
        self.assertIn("missing file", self.check('![image](absent.png)')[1][0])

    def test_cli_explicit_file_and_default_set(self):
        (self.root / "docs").mkdir()
        (self.root / "docs/good.md").write_text("Good")
        self.readme.write_text('[broken](missing.md)')
        with patch.object(check_docs, "ROOT", self.root), contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
            self.assertEqual(check_docs.main(["docs/good.md"]), 0)
            self.assertEqual(check_docs.main([]), 1)
            self.assertEqual(check_docs.main(["../outside.md"]), 1)


if __name__ == "__main__":
    unittest.main()
