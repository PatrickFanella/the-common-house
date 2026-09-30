#!/usr/bin/env python3
"""Generate and return only the Season 1 prop kit from a disposable Editor."""
import hashlib
import json
from pathlib import Path
import shutil
import sys

try:
    from . import unity_common as unity, fetch_polyhaven
except ImportError:
    import unity_common as unity
    import fetch_polyhaven

KIT = "Assets/CommonHouse/Art/SeasonOne"
SCENE = "Assets/CommonHouse/Scenes/SeasonOneAssetReview.unity"


def copy_export(export, root):
    files = sorted(p for p in export.rglob("*") if p.is_file())
    if not files:
        raise ValueError("Editor returned no season assets")
    for path in files:
        relative = path.relative_to(export).as_posix()
        if path.is_symlink() or not (relative.startswith(KIT + "/") or relative in {KIT + ".meta", SCENE, SCENE + ".meta"}):
            raise ValueError(f"Unexpected exported file: {relative}")
    if not (export / SCENE).is_file() or len(list((export / KIT / "Prefabs").glob("*.prefab"))) != 16:
        raise ValueError("Incomplete season export")
    for path in files:
        target = root / path.relative_to(export)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, target)
        if target.suffix in {".prefab", ".mat", ".asset", ".meta", ".unity"}:
            target.write_text("\n".join(line.rstrip() for line in target.read_text().splitlines()) + "\n")
    return len(files)


def record_provenance(root):
    manifest_path = root / "docs/development/asset-provenance.json"
    manifest = json.loads(manifest_path.read_text())
    entries = [{"path": p.relative_to(root).as_posix(), "kind": "generated",
                "generator": "Assets/Editor/SeasonOneAssetKit.cs",
                "sha256": hashlib.sha256(p.read_bytes()).hexdigest(),
                "changes": "Original deterministic 64px season prop surface or generic operating label. No private account text."}
               for p in sorted((root / KIT / "Textures").glob("*.png"))]
    fetch_polyhaven.merge(manifest, entries)
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    return len(entries)


def main():
    evidence = unity.new_evidence("season-assets")
    project = evidence / "project"
    try:
        editor, version = unity.resolve_editor()
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        log = evidence / "editor.log"
        method = "CommonHouse.Editor.SeasonOneAssetKit.GenerateAndExport"
        command = [str(editor), "-batchmode", "-quit", "-force-vulkan", "-projectPath", str(project),
                   "-executeMethod", method, "-logFile", str(log)]
        code = unity.execute(command, evidence, 600, "development")
        if code:
            raise ValueError(f"Season generation failed with exit {code}")
        unity.validate_log(log, require_import=True)
        count = copy_export(evidence / "test-data/export", unity.ROOT)
        textures = record_provenance(unity.ROOT)
        unity.receipt(evidence, status="passed", editor_version=version, exported_files=count, textures=textures)
        print(f"PASS: 16 season prefabs, {textures} original textures. Evidence: {evidence}")
        return 0
    except (OSError, ValueError) as error:
        unity.receipt(evidence, status="failed", error=str(error))
        print(f"FAIL: {error}. Evidence: {evidence}", file=sys.stderr)
        return 1
    finally:
        if project.exists():
            shutil.rmtree(project)


if __name__ == "__main__":
    raise SystemExit(main())
