#!/usr/bin/env python3
"""Generate the original city prefabs/review scene and record their PNG provenance."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

try:
    from . import build_outpost, fetch_polyhaven
except ImportError:
    import build_outpost
    import fetch_polyhaven

ROOT = Path(__file__).resolve().parents[1]
KIT = ROOT / "Assets/CommonHouse/Art/CityKit"


def main():
    protected = [ROOT / "Assets/CommonHouse/Scenes/OutpostBlockout.unity",
                 ROOT / "ProjectSettings/EditorBuildSettings.asset"]
    before = {p: p.read_bytes() for p in protected}
    code = subprocess.run([sys.executable, str(ROOT / "scripts/run_editor_method.py"),
                           "CommonHouse.Editor.CityAssetKit.Generate", "--copy-back"]).returncode
    if code:
        return code
    build_outpost.strip_yaml_whitespace()
    if any(p.read_bytes() != data for p, data in before.items()):
        print("FAIL: city generation changed the outpost or build settings", file=sys.stderr)
        return 1
    manifest_path = ROOT / "docs/development/asset-provenance.json"
    manifest = json.loads(manifest_path.read_text())
    entries = [{"path": p.relative_to(ROOT).as_posix(), "kind": "generated",
                "generator": "Assets/Editor/CityAssetKit.cs",
                "sha256": hashlib.sha256(p.read_bytes()).hexdigest(),
                "changes": "Original deterministic 64px city surface or generic sign for reusable flashback environment studies."}
               for p in sorted((KIT / "Textures").glob("*.png"))]
    fetch_polyhaven.merge(manifest, entries)
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    print(f"PASS: {len(list((KIT / 'Prefabs').glob('*.prefab')))} city prefabs, {len(entries)} original textures; outpost/build settings unchanged")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
