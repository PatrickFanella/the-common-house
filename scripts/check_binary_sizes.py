#!/usr/bin/env python3
"""Fail when a tracked or staged binary file exceeds the plain-Git size cap."""
import argparse
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
LIMIT = 5 * 1024 * 1024  # See decision D-18 in docs/design/decisions-and-open-questions.md.
SNIFF = 8192


def tracked(root):
    result = subprocess.run(["git", "-C", str(root), "ls-files", "-z", "--cached"],
                            capture_output=True, check=True)
    return [root / name for name in result.stdout.decode().split("\0") if name]


def is_binary(path):
    with path.open("rb") as handle:
        return b"\0" in handle.read(SNIFF)


def check(paths, root, limit=LIMIT):
    errors = []
    binaries = 0
    for path in paths:
        if path.is_symlink() or not path.is_file() or not is_binary(path):
            continue
        binaries += 1
        size = path.stat().st_size
        if size > limit:
            errors.append(f"{path.relative_to(root)}: {size} bytes exceeds {limit}-byte binary cap")
    return binaries, errors


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--limit", type=int, default=LIMIT, help="Maximum bytes per binary file.")
    args = parser.parse_args(argv)
    binaries, errors = check(tracked(ROOT), ROOT, args.limit)
    for error in errors:
        print(f"FAIL: {error}", file=sys.stderr)
    if errors:
        return 1
    print(f"PASS: {binaries} binary files within {args.limit} bytes each")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
