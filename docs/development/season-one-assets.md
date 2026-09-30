# Season 1 reusable props

[Asset inventory](../design/season-01-asset-inventory.md) · [Sourcing rules](asset-sourcing.md)

`Assets/CommonHouse/Art/SeasonOne/` holds 16 original low-poly prefabs, shared saved materials, 64×64 point-filtered textures, and two reusable meshes. All materials use `CommonHouse/Retro`. The kit uses existing HDRP and the stable perspective UV treatment. It adds no screen-space distortion filter.

Open `Assets/CommonHouse/Scenes/SeasonOneAssetReview.unity` in the pinned Unity Editor to review the objects. The scene arranges them on plinths, one metre-scale prefab per position. It is excluded from the playable build. It contains no case session or player controller.

| Prefab ID | Size or placement | Intended use |
|---|---|---|
| label-printer | 0.30 m shell width, tabletop | Dev's repairs and blank credit labels |
| off-air-switch | 0.26 m wide, tabletop | Kit's broadcast desk |
| diagnostic-microphone | About 0.37 m tall, tabletop | Min's local diagnostic input |
| covered-microphone | Same stand with an opaque cover | Visible restricted-input trace |
| loose-patch-lead | About 0.44 m wide, tabletop | Two visibly free plugs for the optional disconnection trace |
| folder-closed | 0.27 m wide, tabletop | Retained evidence with hidden insert |
| folder-open | About 0.55 m wide, tabletop | Blank insert exposed for a future reader |
| contact-ledger | 0.45 m wide, tabletop | Blank ruled shared records |
| roster-board | 0.52 m wide, 0.71 m tall | Wall-mounted roster; origin is its bottom edge |
| restricted-input-card | 0.18 m wide, tabletop | Generic ASK marker in a clip |
| chess-board | 0.42 m wide, tabletop | Improvised hardware pawns and a repaired board |
| biscuit-tin | About 0.44 m wide with lid beside it | Ordinary hospitality and radio banter |
| saved-meal | 0.26 m plate and a separate knife | A covered portion kept for someone |
| folding-chair | About 0.85 m tall, floor | Spare shared seating with repair tape |
| station-card | 0.18 m wide, tabletop | Generic LOCAL marker in a clip |
| exterior-camera | About 0.29 m tall | Hooded source-camera study; adapt mounting to the source setting |

Origins sit at the supporting surface. One Unity unit is one metre. Front faces point toward local -Z. Props are presentation assets without colliders, interaction scripts, recording behaviour, or automatic permissions. Add appropriate collision and a separately scoped reader/controller when placing an asset in a playable room. The chair is a static open silhouette, not a folding or seating animation.

## Regeneration

```sh
python3 scripts/build_season_assets.py
```

The wrapper uses the existing licensed Editor runner and a disposable source snapshot. `SeasonOneAssetKit.GenerateAndExport` exports only this kit and its review scene. Python validates the complete export before copying it and records generated PNG hashes in the asset provenance manifest. It never copies back Packages or ProjectSettings. Existing outpost and intro scenes are outside the allowed export.

The Editor menu `Common House/Season One/Generate Props And Review Scene` generates directly in the open project after a scene-save prompt. Prefer the wrapper for reproducible work. Regeneration preserves existing asset GUIDs. It does not remove obsolete kit files automatically.

Before committing generated textures, run `git lfs install`, stage the intended files, and run provenance and binary-size checks. Meshes and prefabs are text assets. Labels are original bitmap lettering; no font, logo, or third-party art has been added.

## Focused checks

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.SeasonOneAssetKitTests --graphics --timeout 600
python3 -m unittest discover -s tests -p test_build_season_assets.py
python3 scripts/check_provenance.py
python3 scripts/check_binary_sizes.py
python3 scripts/check_docs.py
```

The native fixture checks persistent geometry/material references, metre-scale bounds, physical privacy variants, forward label UVs, texture settings, and the separate review scene. It retains eight Editor-rendered images covering all 16 props and closer operating labels. Inspect those images before claiming visual suitability. Scene and import checks do not establish episode gameplay, a player build, or a human playtest.

See the [verification report and rendered review](../verification/season-one-assets.md)
for actual local results and qualification limits.
