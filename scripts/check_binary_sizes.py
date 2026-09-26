#!/usr/bin/env python3
"""Fail when a plain-Git binary exceeds the size cap or an LFS path is committed without a pointer."""
import argparse
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
LIMIT = 5 * 1024 * 1024  # See decisions D-18 and D-22 in docs/design/decisions-and-open-questions.md.
SNIFF = 8192
POINTER_PREFIX = b"version https://git-lfs.github.com/spec/v1\n"


def tracked(root):
    result = subprocess.run(["git", "-C", str(root), "ls-files", "-z", "--cached"],
                            capture_output=True, check=True)
    return [root / name for name in result.stdout.decode().split("\0") if name]


def lfs_tracked(root, paths):
    """Return the subset of paths whose .gitattributes filter is lfs."""
    if not paths:
        return set()
    names = "\0".join(str(path.relative_to(root)) for path in paths) + "\0"
    result = subprocess.run(["git", "-C", str(root), "check-attr", "-z", "--stdin", "filter"],
                            input=names.encode(), capture_output=True, check=True)
    fields = result.stdout.decode().split("\0")
    return {root / fields[i] for i in range(0, len(fields) - 2, 3) if fields[i + 2] == "lfs"}


def staged_blob_is_pointer(root, path):
    result = subprocess.run(["git", "-C", str(root), "cat-file", "blob", ":" + str(path.relative_to(root))],
                            capture_output=True)
    return result.returncode == 0 and result.stdout.startswith(POINTER_PREFIX)


def is_binary(path):
    with path.open("rb") as handle:
        return b"\0" in handle.read(SNIFF)


def check(paths, root, limit=LIMIT, lfs=frozenset(), is_pointer=None):
    errors = []
    binaries = 0
    for path in paths:
        if path in lfs:
            if is_pointer is not None and not is_pointer(path):
                errors.append(f"{path.relative_to(root)}: matches an LFS pattern but was committed without "
                              "an LFS pointer; run `git lfs install` and add it again")
            continue
        if path.is_symlink() or not path.is_file() or not is_binary(path):
            continue
        binaries += 1
        size = path.stat().st_size
        if size > limit:
            errors.append(f"{path.relative_to(root)}: {size} bytes exceeds {limit}-byte binary cap")
    return binaries, errors


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--limit", type=int, default=LIMIT, help="Maximum bytes per plain-Git binary file.")
    args = parser.parse_args(argv)
    paths = tracked(ROOT)
    lfs = lfs_tracked(ROOT, paths)
    binaries, errors = check(paths, ROOT, args.limit, lfs, lambda path: staged_blob_is_pointer(ROOT, path))
    for error in errors:
        print(f"FAIL: {error}", file=sys.stderr)
    if errors:
        return 1
    print(f"PASS: {binaries} plain-Git binary files within {args.limit} bytes each; {len(lfs)} LFS files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
