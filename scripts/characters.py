#!/usr/bin/env python3
"""Set up the pinned character toolchain and build characters from recipes (decision D-23).

    python3 scripts/characters.py setup
    python3 scripts/characters.py build [ID ...]
    python3 scripts/characters.py retro [ID ...]

`setup` downloads Blender, the MPFB extension, and the MakeHuman CC0 asset packs, checks each
against a pinned SHA-256, and installs them into a Blender profile used only by this project.
`build` runs tools/characters/mpfb_build.py for each recipe in tools/characters/recipes/ (or
the named ones), then copies the FBX, its character description, and its textures into
Assets/CommonHouse/Art/Characters/ and records their provenance. `retro` builds the low-poly
version used by the D-24 look with tools/characters/retro_bake.py: MPFB's proxy741 body with the
detailed character baked onto one small texture, in Assets/CommonHouse/Art/Characters/Retro/. Rebuilding a recipe with the
same pins reproduces the same character.
"""
import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import urllib.request
import zipfile

try:
    from . import fetch_polyhaven
except ImportError:
    import fetch_polyhaven

ROOT = Path(__file__).resolve().parents[1]
RECIPES = ROOT / "tools/characters/recipes"
BUILD_SCRIPT = "tools/characters/mpfb_build.py"
RETRO_SCRIPT = "tools/characters/retro_bake.py"
RETRO_TEXTURE_SIZE = 128
INSTALL_SCRIPT = "tools/characters/mpfb_install_packs.py"
CHARACTERS = "Assets/CommonHouse/Art/Characters"
MANIFEST = ROOT / "docs/development/asset-provenance.json"
HOME = Path.home()
CACHE = HOME / ".cache/common-house-blender"
BLENDER_DIR = HOME / ".local/opt/blender-4.5.14-linux-x64"
PROFILE = HOME / ".local/share/common-house-blender"
USER_AGENT = "Mozilla/5.0 (CommonHouse-asset-sourcing/1.0)"

BLENDER = {
    "file": "blender-4.5.14-linux-x64.tar.xz",
    "urls": ["https://ftp.nluug.nl/pub/graphics/blender/release/Blender4.5/blender-4.5.14-linux-x64.tar.xz",
             "https://download.blender.org/release/Blender4.5/blender-4.5.14-linux-x64.tar.xz"],
    "sha256": "9ba871ff2ecd36526b77432745980b7e6664ecd0c7ca11c48849073dcfe06da3",
}
MPFB = {
    "file": "add-on-mpfb-v2.0.17.zip",
    "version": "2.0.17",
    "urls": ["https://extensions.blender.org/download/sha256:4f0a879d64a39bf646fbf5f53601ac678855da329d650617dca5737548239a87/add-on-mpfb-v2.0.17.zip"],
    "sha256": "4f0a879d64a39bf646fbf5f53601ac678855da329d650617dca5737548239a87",
}
PACK_PAGE = "https://static.makehumancommunity.org/assets/assetpacks/{}.html"
PACKS = {
    # name: (SHA-256 of the CC0 zip, date first retrieved)
    "makehuman_system_assets": ("b542127a8e25547c7c29c19f2d1d2adb9a664c80396ecd694095dbc8028a0107", "2026-09-28"),
    "skins01": ("7495ab99287053bd19ff1636114e64b608994d9f7437fea6cc75ea387f96dba9", "2026-09-28"),
    "skins02": ("1613f1ef3afca53094511d26620ed7cf1d2dedc29ed3d384d60bdebe250698ae", "2026-09-28"),
}


def pack_file(name):
    return f"{name}_cc0.zip"


def pack_urls(name):
    return [f"https://files.makehumancommunity.org/asset_packs/{name}/{pack_file(name)}",
            f"https://files2.makehumancommunity.org/asset_packs/{name}/{pack_file(name)}"]


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for block in iter(lambda: handle.read(1 << 20), b""):
            digest.update(block)
    return digest.hexdigest()


def cached(name, urls, expected):
    """Return a cached download whose SHA-256 matches, fetching it from the first working URL."""
    CACHE.mkdir(parents=True, exist_ok=True)
    path = CACHE / name
    if path.is_file() and sha256(path) == expected:
        return path
    errors = []
    for url in urls:
        try:
            request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            partial = path.with_suffix(path.suffix + ".part")
            with urllib.request.urlopen(request, timeout=600) as response, open(partial, "wb") as handle:
                shutil.copyfileobj(response, handle)
            if sha256(partial) != expected:
                errors.append(f"{url}: SHA-256 mismatch")
                partial.unlink()
                continue
            partial.replace(path)
            return path
        except OSError as error:
            errors.append(f"{url}: {error}")
    raise ValueError(f"cannot download {name}: " + "; ".join(errors))


def blender_env():
    env = dict(os.environ)
    env["BLENDER_USER_RESOURCES"] = str(PROFILE)
    return env


def blender(*args, timeout=900):
    command = [str(BLENDER_DIR / "blender"), *args]
    result = subprocess.run(command, env=blender_env(), capture_output=True, text=True, timeout=timeout)
    output = result.stdout + result.stderr
    if result.returncode != 0 or "Traceback" in output or "FAILED:" in output:
        tail = "\n".join(output.splitlines()[-25:])
        raise ValueError(f"Blender failed ({' '.join(args[:3])} ...):\n{tail}")
    return output


def setup():
    tarball = cached(BLENDER["file"], BLENDER["urls"], BLENDER["sha256"])
    if not (BLENDER_DIR / "blender").is_file():
        BLENDER_DIR.parent.mkdir(parents=True, exist_ok=True)
        subprocess.run(["tar", "-xf", str(tarball), "-C", str(BLENDER_DIR.parent)], check=True)
    PROFILE.mkdir(parents=True, exist_ok=True)
    extension = cached(MPFB["file"], MPFB["urls"], MPFB["sha256"])
    blender("--command", "extension", "install-file", "-r", "user_default", "-e", str(extension))
    packs = [str(cached(pack_file(name), pack_urls(name), digest)) for name, (digest, _) in PACKS.items()]
    blender("-b", "--python", str(ROOT / INSTALL_SCRIPT), "--", *packs)
    print(f"PASS: Blender 4.5.14, MPFB {MPFB['version']}, and {len(PACKS)} asset packs installed in {PROFILE}")


def pack_index():
    """Maps each file path inside the installed packs to its pack name."""
    index = {}
    for name, (digest, _) in PACKS.items():
        path = CACHE / pack_file(name)
        if not path.is_file() or sha256(path) != digest:
            raise ValueError(f"asset pack {name} is missing from the cache; run `python3 scripts/characters.py setup`")
        with zipfile.ZipFile(path) as archive:
            for member in archive.namelist():
                if not member.endswith("/"):
                    index.setdefault(member, name)
    return index


def provenance_entries(character, description, fbx_path, index):
    entries = [{
        "path": fbx_path,
        "kind": "generated",
        "generator": BUILD_SCRIPT,
        "sha256": sha256(ROOT / fbx_path),
        "changes": f"MPFB {MPFB['version']} export of tools/characters/recipes/{character}.json with MakeHuman CC0 assets; "
                   f"built by scripts/characters.py in Blender 4.5.14.",
    }]
    for texture in description["textures"]:
        relative, authors = texture["path"], texture["authors"]
        pack = index.get(relative)
        if pack is None:
            raise ValueError(f"{relative} is not in any pinned asset pack")
        path = f"{CHARACTERS}/Textures/{relative}"
        entries.append({
            "path": path,
            "kind": "cc0",
            "source": "MakeHuman",
            "url": PACK_PAGE.format(pack),
            "authors": authors,
            "license": "CC0-1.0",
            "retrieved": PACKS[pack][1],
            "sha256": sha256(ROOT / path),
        })
    return entries


def build(ids):
    if not (BLENDER_DIR / "blender").is_file():
        raise ValueError("Blender is not installed; run `python3 scripts/characters.py setup`")
    recipes = sorted(RECIPES.glob("*.json"))
    if ids:
        wanted = set(ids)
        recipes = [recipe for recipe in recipes if recipe.stem in wanted]
        missing = wanted - {recipe.stem for recipe in recipes}
        if missing:
            raise ValueError("no recipe for " + ", ".join(sorted(missing)))
    if not recipes:
        raise ValueError("no recipes to build")
    index = pack_index()
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    for recipe in recipes:
        character = json.loads(recipe.read_text(encoding="utf-8"))["id"]
        if character != recipe.stem:
            raise ValueError(f"{recipe.name}: id '{character}' must match the file name")
        with tempfile.TemporaryDirectory(prefix="common-house-character-") as temp:
            blender("-b", "--python", str(ROOT / BUILD_SCRIPT), "--", str(recipe), temp)
            output = Path(temp)
            target = ROOT / CHARACTERS / character
            target.mkdir(parents=True, exist_ok=True)
            for suffix in (".fbx", ".character.json"):
                shutil.copyfile(output / (character + suffix), target / (character + suffix))
            for texture in (output / "Textures").rglob("*"):
                if texture.is_file():
                    destination = ROOT / CHARACTERS / "Textures" / texture.relative_to(output / "Textures")
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(texture, destination)
        description = json.loads((target / (character + ".character.json")).read_text(encoding="utf-8"))
        fetch_polyhaven.merge(manifest, provenance_entries(character, description, f"{CHARACTERS}/{character}/{character}.fbx", index))
        print(f"{character}: built with {len(description['textures'])} textures")
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: built {len(recipes)} character(s). Import them in Unity and run the checks.")


def selected_recipes(ids):
    recipes = sorted(RECIPES.glob("*.json"))
    if ids:
        wanted = set(ids)
        recipes = [recipe for recipe in recipes if recipe.stem in wanted]
        missing = wanted - {recipe.stem for recipe in recipes}
        if missing:
            raise ValueError("no recipe for " + ", ".join(sorted(missing)))
    if not recipes:
        raise ValueError("no recipes to build")
    return recipes


def retro_entries(character, description, folder):
    packs = sorted({pack for pack in (pack_index().get(source) for source in description["sources"]) if pack})
    origin = (f"tools/characters/recipes/{character}.json with MakeHuman CC0 assets ({', '.join(packs)}), "
              f"MPFB {MPFB['version']} proxy741 body")
    return [
        {"path": f"{folder}/{character}.fbx", "kind": "generated", "generator": RETRO_SCRIPT,
         "sha256": sha256(ROOT / folder / f"{character}.fbx"),
         "changes": f"Low-poly export of {origin}; built by scripts/characters.py retro in Blender 4.5.14."},
        {"path": f"{folder}/{character}_albedo.png", "kind": "generated", "generator": RETRO_SCRIPT,
         "sha256": sha256(ROOT / folder / f"{character}_albedo.png"),
         "changes": f"Colour of the detailed character from {origin} baked onto the proxy's UVs with Cycles, "
                    f"shrunk to {description['texture_size']} px."},
    ]


def retro(ids):
    if not (BLENDER_DIR / "blender").is_file():
        raise ValueError("Blender is not installed; run `python3 scripts/characters.py setup`")
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    recipes = selected_recipes(ids)
    for recipe in recipes:
        character = json.loads(recipe.read_text(encoding="utf-8"))["id"]
        folder = f"{CHARACTERS}/Retro/{character}"
        with tempfile.TemporaryDirectory(prefix="common-house-retro-") as temp:
            blender("-b", "--python", str(ROOT / RETRO_SCRIPT), "--", str(recipe), temp, str(RETRO_TEXTURE_SIZE))
            target = ROOT / folder
            target.mkdir(parents=True, exist_ok=True)
            for suffix in (".fbx", "_albedo.png", ".retro.json"):
                shutil.copyfile(Path(temp) / (character + suffix), target / (character + suffix))
        description = json.loads((ROOT / folder / f"{character}.retro.json").read_text(encoding="utf-8"))
        fetch_polyhaven.merge(manifest, retro_entries(character, description, folder))
        print(f"{character}: {description['faces']} faces, {description['texture_size']} px texture")
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: built {len(recipes)} retro character(s).")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("setup", help="Install the pinned Blender, MPFB, and asset packs.")
    build_parser = commands.add_parser("build", help="Build characters from recipes.")
    build_parser.add_argument("ids", nargs="*", help="Recipe IDs (default: all).")
    retro_parser = commands.add_parser("retro", help="Build low-poly retro characters from recipes.")
    retro_parser.add_argument("ids", nargs="*", help="Recipe IDs (default: all).")
    args = parser.parse_args(argv)
    try:
        if args.command == "setup":
            setup()
        elif args.command == "retro":
            retro(args.ids)
        else:
            build(args.ids)
    except (OSError, ValueError, subprocess.SubprocessError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
