#!/usr/bin/env python3
"""Import animations downloaded from Mixamo and record their provenance (decision D-25).

    python3 scripts/import_mixamo.py [FOLDER]      # default: ~/Downloads/mixamo

Mixamo downloads need an Adobe account, so the owner downloads each animation as
"FBX for Unity (.fbx)", "Without Skin", 30 frames per second, into FOLDER. This script copies
each .fbx into Assets/CommonHouse/Art/Animations/Mixamo/ under a lowercase, hyphenated name and
writes a `licensed` provenance entry dated to the file's download time. Mixamo animations may be
used royalty-free in the game, but the raw files may not be redistributed: they stay in this
private repository and must be removed before it is ever published.
"""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys

try:
    from . import fetch_polyhaven
except ImportError:
    import fetch_polyhaven

ROOT = Path(__file__).resolve().parents[1]
DESTINATION = "Assets/CommonHouse/Art/Animations/Mixamo"
MANIFEST = ROOT / "docs/development/asset-provenance.json"
FBX_BINARY = b"Kaydara FBX Binary"


def slug(name):
    """'Breathing Idle (1).fbx' -> 'breathing-idle-1'."""
    stem = Path(name).stem.lower()
    return re.sub(r"[^a-z0-9]+", "-", stem).strip("-")


def entry(path, data, retrieved):
    return {
        "path": path,
        "kind": "licensed",
        "source": "Mixamo",
        "url": "https://www.mixamo.com/",
        "license": "Adobe-Mixamo-Terms",
        "retrieved": retrieved,
        "sha256": hashlib.sha256(data).hexdigest(),
    }


def import_folder(folder):
    files = sorted(Path(folder).glob("*.fbx"))
    if not files:
        raise ValueError(f"no .fbx files in {folder}")
    entries = []
    target_folder = ROOT / DESTINATION
    target_folder.mkdir(parents=True, exist_ok=True)
    for source in files:
        data = source.read_bytes()
        if not data.startswith(FBX_BINARY):
            raise ValueError(f"{source.name} is not a binary FBX; download 'FBX for Unity (.fbx)'")
        name = slug(source.name)
        if not name:
            raise ValueError(f"cannot make a file name from {source.name}")
        target = target_folder / f"{name}.fbx"
        shutil.copyfile(source, target)
        retrieved = datetime.date.fromtimestamp(source.stat().st_mtime).isoformat()
        entries.append(entry(f"{DESTINATION}/{name}.fbx", data, retrieved))
    return entries


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("folder", nargs="?", default=str(Path.home() / "Downloads/mixamo"))
    args = parser.parse_args(argv)
    try:
        entries = import_folder(args.folder)
    except (OSError, ValueError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    fetch_polyhaven.merge(manifest, entries)
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    for item in entries:
        print(f"imported {item['path']}")
    print(f"PASS: {len(entries)} Mixamo animation(s) imported. Next: python3 scripts/build_outpost.py")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
