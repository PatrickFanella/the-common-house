#!/usr/bin/env python3
"""Download CC0 Poly Haven textures or models and record their provenance (decision D-21).

For each asset, fetches the diffuse, OpenGL normal, and ARM maps as JPG at the chosen
resolution, plus the FBX mesh for models. Every file is checked against the MD5 that Poly
Haven publishes, written under Assets/CommonHouse/Art/Sourced/PolyHaven/<asset>/, and given
an entry in the provenance manifest. Run `git lfs install` once per clone before committing.

    python3 scripts/fetch_polyhaven.py vintage_radio_transceiver metal_office_desk:1k
"""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import sys
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
DESTINATION = "Assets/CommonHouse/Art/Sourced/PolyHaven"
MANIFEST = ROOT / "docs/development/asset-provenance.json"
API = "https://api.polyhaven.com"
USER_AGENT = "CommonHouse-asset-sourcing/1.0"
MAP_SUFFIXES = ("diff", "nor_gl", "arm")
MODEL_TYPE = 2


def fetch(url):
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(request, timeout=120) as response:
        return response.read()


def is_map_key(key):
    """Selects the maps HDRP needs: 'Diffuse', 'nor_gl', 'arm', and per-part '<part>_diff' etc."""
    return key in ("Diffuse", "nor_gl", "arm") or any(key.endswith("_" + suffix) for suffix in MAP_SUFFIXES)


def plan(files, asset_type, resolution):
    """Return [(file name, url, md5)] for one asset's listing from the /files endpoint."""
    wanted = []
    for key in sorted(files):
        if not is_map_key(key):
            continue
        try:
            entry = files[key][resolution]["jpg"]
        except KeyError as error:
            raise ValueError(f"{key} has no {resolution} JPG") from error
        wanted.append((entry["url"].rsplit("/", 1)[-1], entry["url"], entry["md5"]))
    if asset_type == MODEL_TYPE:
        try:
            entry = files["fbx"][resolution]["fbx"]
        except KeyError as error:
            raise ValueError(f"no {resolution} FBX") from error
        wanted.append((entry["url"].rsplit("/", 1)[-1], entry["url"], entry["md5"]))
    if not wanted:
        raise ValueError("no downloadable maps found")
    return wanted


def entry(path, asset, authors, data, retrieved):
    return {
        "path": path,
        "kind": "cc0",
        "source": "Poly Haven",
        "url": f"https://polyhaven.com/a/{asset}",
        "authors": authors,
        "license": "CC0-1.0",
        "retrieved": retrieved,
        "sha256": hashlib.sha256(data).hexdigest(),
    }


def merge(manifest, entries):
    """Replace entries with the same path and keep the list sorted by path."""
    by_path = {item["path"]: item for item in manifest["assets"]}
    for item in entries:
        by_path[item["path"]] = item
    manifest["assets"] = [by_path[path] for path in sorted(by_path)]
    return manifest


def download(asset, resolution, retrieved, get=fetch):
    info = json.loads(get(f"{API}/info/{asset}"))
    files = json.loads(get(f"{API}/files/{asset}"))
    authors = sorted(info.get("authors", {}))
    if not authors:
        raise ValueError(f"{asset}: Poly Haven lists no authors")
    folder = ROOT / DESTINATION / asset
    folder.mkdir(parents=True, exist_ok=True)
    entries = []
    for name, url, md5 in plan(files, info.get("type"), resolution):
        data = get(url)
        if hashlib.md5(data).hexdigest() != md5:
            raise ValueError(f"{name}: MD5 does not match Poly Haven's listing")
        (folder / name).write_bytes(data)
        entries.append(entry(f"{DESTINATION}/{asset}/{name}", asset, authors, data, retrieved))
    return entries


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("assets", nargs="+", help="Poly Haven asset IDs, optionally with :1k, :2k, or :4k (default 2k).")
    args = parser.parse_args(argv)
    retrieved = datetime.date.today().isoformat()
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    try:
        for spec in args.assets:
            asset, _, resolution = spec.partition(":")
            entries = download(asset, resolution or "2k", retrieved)
            merge(manifest, entries)
            print(f"{asset}: {len(entries)} files")
    except (OSError, ValueError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    finally:
        MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
