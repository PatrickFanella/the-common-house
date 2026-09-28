#!/usr/bin/env python3
"""Build the experimental N64 outpost scene (issue #67) and record provenance for its textures.

Decimates each prop listed in tools/experiments/n64_targets.json with Blender, runs
CommonHouse.Editor.Experiments.N64Outpost.Build in a disposable project copy, copies the
result back, strips Unity's trailing whitespace from changed YAML, and adds a `generated`
provenance entry for every downsampled texture listed in the build's sources.json.
"""
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
SOURCES = "Assets/CommonHouse/Experiments/N64/Textures/sources.json"
MODELS = "Assets/CommonHouse/Experiments/N64/Models"
TARGETS = ROOT / "tools/experiments/n64_targets.json"
DECIMATE = "tools/experiments/n64_decimate.py"
POLYHAVEN = "Assets/CommonHouse/Art/Sourced/PolyHaven"
CHARACTERS = "Assets/CommonHouse/Art/Characters"
N64_CHARACTERS = "Assets/CommonHouse/Experiments/N64/Characters"
# GoldenEye characters were a few hundred triangles; clothes over a body need somewhat more.
CHARACTER_TRIANGLES = 1200
GENERATOR = "Assets/Editor/Experiments/N64Outpost.cs"
MANIFEST = ROOT / "docs/development/asset-provenance.json"

try:
    from . import characters, fetch_polyhaven
except ImportError:
    import characters
    import fetch_polyhaven


def strip_yaml_whitespace():
    changed = subprocess.run(["git", "-C", str(ROOT), "ls-files", "-m", "-o", "--exclude-standard"],
                             capture_output=True, text=True, check=True).stdout.split()
    for name in changed:
        if name.endswith((".asset", ".unity", ".meta", ".mat", ".prefab")):
            path = ROOT / name
            text = path.read_text(encoding="utf-8")
            cleaned = re.sub(r"[ \t]+\n", "\n", text)
            if cleaned != text:
                path.write_text(cleaned, encoding="utf-8")


def entries(sources):
    result = []
    for item in sources["textures"]:
        path = ROOT / item["texture"]
        result.append({
            "path": item["texture"],
            "kind": "generated",
            "generator": GENERATOR,
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "changes": f"Area-averaged to 64 px from {item['source']} for the N64 experiment.",
        })
    return result


def decimate():
    """Reduce each prop to its triangle target with Blender; returns provenance entries."""
    if not (characters.BLENDER_DIR / "blender").is_file():
        raise SystemExit("Blender is not installed; run `python3 scripts/characters.py setup`")
    result = []
    (ROOT / MODELS).mkdir(parents=True, exist_ok=True)
    jobs = [(next((ROOT / POLYHAVEN / asset).glob("*.fbx")), ROOT / MODELS / f"{asset}.fbx", triangles)
            for asset, triangles in sorted(json.loads(TARGETS.read_text(encoding="utf-8")).items())]
    (ROOT / N64_CHARACTERS).mkdir(parents=True, exist_ok=True)
    jobs += [(fbx, ROOT / N64_CHARACTERS / fbx.name, CHARACTER_TRIANGLES)
             for fbx in sorted((ROOT / CHARACTERS).glob("*/*.fbx"))]
    for source, target, triangles in jobs:
        output = characters.blender("-b", "--python", str(ROOT / DECIMATE), "--", str(source), str(target), str(triangles))
        print(next(line for line in output.splitlines() if line.startswith("DECIMATED")))
        result.append({
            "path": str(target.relative_to(ROOT)),
            "kind": "generated",
            "generator": DECIMATE,
            "sha256": hashlib.sha256(target.read_bytes()).hexdigest(),
            "changes": f"Decimated to about {triangles} triangles from {source.relative_to(ROOT)} for the N64 experiment.",
        })
    return result


GENERATED = ["Textures", "Materials", "Models", "Characters", "OutpostN64.unity", "N64VolumeProfile.asset"]


FACES = "Assets/CommonHouse/Experiments/Faces"
FACE_GENERATOR = "Assets/Editor/Experiments/FacePhotos.cs"


def face_entries():
    result = []
    for photo in sorted((ROOT / FACES).glob("*_face.png")):
        character = photo.name[: -len("_face.png")]
        result.append({
            "path": str(photo.relative_to(ROOT)),
            "kind": "generated",
            "generator": FACE_GENERATOR,
            "sha256": hashlib.sha256(photo.read_bytes()).hexdigest(),
            "changes": f"Frontal render of the generated character {character} (MakeHuman CC0 assets), shrunk to 64x80 with an oval mask.",
        })
    return result


def clean():
    """Remove earlier outputs, so files dropped from the build do not linger without provenance."""
    base = ROOT / "Assets/CommonHouse/Experiments/N64"
    for path in (ROOT / FACES, ROOT / (FACES + ".meta")):
        if path.is_dir():
            shutil.rmtree(path)
        elif path.exists():
            path.unlink()
    for name in GENERATED:
        for path in (base / name, base / (name + ".meta")):
            if path.is_dir():
                shutil.rmtree(path)
            elif path.exists():
                path.unlink()


def main():
    clean()
    models = decimate()
    code = subprocess.run([sys.executable, str(ROOT / "scripts/run_editor_method.py"),
                           "CommonHouse.Editor.Experiments.N64Outpost.Build", "--copy-back"]).returncode
    if code:
        return code
    subprocess.run(["git", "-C", str(ROOT), "checkout", "--", "ProjectSettings/ShaderGraphSettings.asset"], check=False)
    strip_yaml_whitespace()
    sources = json.loads((ROOT / SOURCES).read_text(encoding="utf-8"))
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    manifest["assets"] = [item for item in manifest["assets"]
                          if not item["path"].startswith(("Assets/CommonHouse/Experiments/N64/", FACES + "/"))]
    fetch_polyhaven.merge(manifest, entries(sources) + models + face_entries())
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: N64 outpost built with {len(sources['textures'])} downsampled textures")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
