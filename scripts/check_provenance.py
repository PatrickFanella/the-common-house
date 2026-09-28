#!/usr/bin/env python3
"""Fail unless every sourced or LFS asset file has a matching provenance entry (decision D-21)."""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import re
import sys
from urllib.parse import urlparse

try:
    from . import check_binary_sizes as git_files
except ImportError:
    import check_binary_sizes as git_files

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = "docs/development/asset-provenance.json"
SOURCED_ROOT = "Assets/CommonHouse/Art/Sourced/"
# Approved external sources and the host their asset pages must use. Extend only with a recorded decision.
SOURCES = {"Poly Haven": "polyhaven.com", "ambientCG": "ambientcg.com", "MakeHuman": "makehumancommunity.org"}
LICENSES = {"CC0-1.0"}
COMMON_FIELDS = {"path", "kind", "sha256"}
KIND_FIELDS = {
    "cc0": {"source", "url", "authors", "license", "retrieved"},
    "generated": {"generator"},
}
OPTIONAL_FIELDS = {"changes"}
SHA256 = re.compile(r"[0-9a-f]{64}")


def content_sha256(path):
    """SHA-256 of the file's content; for an unfetched LFS pointer, the oid it names."""
    data = path.read_bytes()
    if data.startswith(git_files.POINTER_PREFIX):
        match = re.search(rb"^oid sha256:([0-9a-f]{64})$", data, re.MULTILINE)
        if match:
            return match.group(1).decode()
    return hashlib.sha256(data).hexdigest()


def required_paths(tracked, lfs, root):
    required = set(lfs)
    for path in tracked:
        name = path.relative_to(root).as_posix()
        if name.startswith(SOURCED_ROOT) and not name.endswith(".meta"):
            required.add(path)
    return required


def entry_errors(entry, root):
    label = entry.get("path", "<entry without path>") if isinstance(entry, dict) else "<non-object entry>"
    if not isinstance(entry, dict):
        return [f"{label}: entry must be an object"]
    kind = entry.get("kind")
    if kind not in KIND_FIELDS:
        return [f"{label}: kind must be one of {sorted(KIND_FIELDS)}"]
    expected = COMMON_FIELDS | KIND_FIELDS[kind]
    errors = []
    missing = expected - entry.keys()
    unknown = entry.keys() - expected - OPTIONAL_FIELDS
    if missing:
        errors.append(f"{label}: missing {sorted(missing)}")
    if unknown:
        errors.append(f"{label}: unknown fields {sorted(unknown)}")
    if missing:
        return errors
    if not SHA256.fullmatch(str(entry["sha256"])):
        errors.append(f"{label}: sha256 must be 64 lowercase hex characters")
    if kind == "cc0":
        host = SOURCES.get(entry["source"])
        if host is None:
            errors.append(f"{label}: source {entry['source']!r} is not one of {sorted(SOURCES)}")
        else:
            url_host = urlparse(str(entry["url"])).hostname or ""
            if urlparse(str(entry["url"])).scheme != "https" or not (url_host == host or url_host.endswith("." + host)):
                errors.append(f"{label}: url must be an https page on {host}")
        if entry["license"] not in LICENSES:
            errors.append(f"{label}: license {entry['license']!r} is not one of {sorted(LICENSES)}")
        if not isinstance(entry["authors"], list) or not entry["authors"] or not all(
                isinstance(author, str) and author.strip() for author in entry["authors"]):
            errors.append(f"{label}: authors must be a non-empty list of names")
        try:
            datetime.date.fromisoformat(str(entry["retrieved"]))
        except ValueError:
            errors.append(f"{label}: retrieved must be an ISO date (YYYY-MM-DD)")
    else:
        if not (root / str(entry["generator"])).is_file():
            errors.append(f"{label}: generator {entry['generator']!r} is not a file in the repository")
    return errors


def check(manifest, root, tracked, lfs):
    """Return (entry count, errors) for a parsed manifest against tracked and LFS paths."""
    if not isinstance(manifest, dict) or manifest.get("version") != 1 or not isinstance(manifest.get("assets"), list):
        return 0, ['manifest must be {"version": 1, "assets": [...]}']
    errors = []
    tracked = set(tracked)
    seen = {}
    for entry in manifest["assets"]:
        problems = entry_errors(entry, root)
        errors.extend(problems)
        if problems:
            continue
        path = root / entry["path"]
        if entry["path"] in seen:
            errors.append(f"{entry['path']}: duplicate provenance entry")
            continue
        seen[entry["path"]] = entry
        if path not in tracked or not path.is_file():
            errors.append(f"{entry['path']}: provenance entry names a file that is not tracked")
        elif content_sha256(path) != entry["sha256"]:
            errors.append(f"{entry['path']}: sha256 does not match the file")
    for path in sorted(required_paths(tracked, lfs, root)):
        name = path.relative_to(root).as_posix()
        if name not in seen:
            errors.append(f"{name}: sourced or LFS file has no provenance entry in {MANIFEST}")
    return len(seen), errors


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.parse_args(argv)
    try:
        manifest = json.loads((ROOT / MANIFEST).read_text(encoding="utf-8"))
    except (OSError, ValueError) as error:
        print(f"FAIL: cannot read {MANIFEST}: {error}", file=sys.stderr)
        return 1
    tracked = git_files.tracked(ROOT)
    count, errors = check(manifest, ROOT, tracked, git_files.lfs_tracked(ROOT, tracked))
    for error in errors:
        print(f"FAIL: {error}", file=sys.stderr)
    if errors:
        return 1
    print(f"PASS: {count} provenance entries cover every sourced and LFS asset file")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
