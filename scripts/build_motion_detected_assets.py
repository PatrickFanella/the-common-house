#!/usr/bin/env python3
"""Generate the exterior camera source set and return only its owned assets."""
import hashlib
import json
import shutil
import sys
try:
    from . import unity_common as unity, fetch_polyhaven, build_season_assets
except ImportError:
    import unity_common as unity
    import fetch_polyhaven
    import build_season_assets

KIT = "Assets/CommonHouse/Art/MotionDetected"
SCENE = "Assets/CommonHouse/Scenes/MotionDetectedSourceReview.unity"


def main():
    evidence = unity.new_evidence("motion-detected-assets")
    project = evidence / "project"
    try:
        editor, version = unity.resolve_editor()
        unity.record_source(evidence, unity.snapshot(unity.ROOT, project))
        log = evidence / "editor.log"
        command = [str(editor), "-batchmode", "-quit", "-force-vulkan", "-projectPath", str(project),
                   "-executeMethod", "CommonHouse.Editor.MotionDetectedSourceSet.GenerateAndExport", "-logFile", str(log)]
        code = unity.execute(command, evidence, 600, "development")
        if code:
            raise ValueError(f"Source set generation failed with exit {code}")
        unity.validate_log(log, require_import=True)
        count = build_season_assets.copy_export(evidence / "test-data/export", unity.ROOT,
                                               kit=KIT, scene=SCENE, expected_prefabs=6)
        path = unity.ROOT / "docs/development/asset-provenance.json"
        manifest = json.loads(path.read_text())
        entries = [{"path": p.relative_to(unity.ROOT).as_posix(), "kind": "generated",
                    "generator": "Assets/Editor/MotionDetectedSourceSet.cs",
                    "sha256": hashlib.sha256(p.read_bytes()).hexdigest(),
                    "changes": "Original deterministic 64px exterior-source material. No indoor or private account imagery."}
                   for p in sorted((unity.ROOT / KIT / "Textures").glob("*.png"))]
        fetch_polyhaven.merge(manifest, entries)
        path.write_text(json.dumps(manifest, indent=2) + "\n")
        unity.receipt(evidence, status="passed", editor_version=version, exported_files=count, textures=len(entries))
        print(f"PASS: 6 exterior prefabs, {len(entries)} original textures. Evidence: {evidence}")
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
