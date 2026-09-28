#!/usr/bin/env python3
"""Build the experimental PS1 outpost scene (issue #69) from the N64 scene.

Runs CommonHouse.Editor.Experiments.PS1Outpost.Build in a disposable project copy and copies the
result back. The PS1 scene reuses the N64 build's models and textures, so it adds no new binary
assets and needs no provenance entries; run scripts/build_n64.py first when those change.
"""
from pathlib import Path
import shutil
import subprocess
import sys

try:
    from . import build_n64
except ImportError:
    import build_n64

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / "Assets/CommonHouse/Experiments/PS1"
GENERATED = ["Materials", "OutpostPS1.unity", "PS1VolumeProfile.asset"]


def main():
    for name in GENERATED:
        for path in (BASE / name, BASE / (name + ".meta")):
            if path.is_dir():
                shutil.rmtree(path)
            elif path.exists():
                path.unlink()
    code = subprocess.run([sys.executable, str(ROOT / "scripts/run_editor_method.py"),
                           "CommonHouse.Editor.Experiments.PS1Outpost.Build", "--copy-back"]).returncode
    if code:
        return code
    subprocess.run(["git", "-C", str(ROOT), "checkout", "--", "ProjectSettings/ShaderGraphSettings.asset"], check=False)
    build_n64.strip_yaml_whitespace()
    print("PASS: PS1 outpost built")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
