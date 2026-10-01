# Motion detected exterior source set

[Episode script](../design/episodes/motion-detected/script.md) · [Facts and coverage contract](../design/episodes/motion-detected/case-contract.md) · [Season asset inventory](../design/season-01-asset-inventory.md)

This provisional set provides the exterior basement entrance recorded by Jo's old network camera. It has no basement interior, character models, performances, or clip playback. It supplies the physical setting for MD-V01, MD-V02, and MD-V03 while retaining their source limits. Native scope is [issue #105](https://git.subcult.tv/PatrickFanella/the-common-house/issues/105).

## Delivered assets

`Assets/CommonHouse/Art/MotionDetected/` contains six original prefabs and nine small original material textures. Materials use `CommonHouse/Retro`; textures are 64×64, point filtered, and stored through Git LFS with generated provenance.

| Prefab | Geometry and origin | Intended observation |
|---|---|---|
| basement-steps | Six 0.20 m risers, 0.30 m treads, 1.50 m width; origin at upper ground level | Descent to a lower basement entrance |
| basement-door-closed | 1.00×2.05 m leaf; origin at threshold centre | Exterior door, handle, frame and kick plate |
| basement-door-open | Same frame and hinge, leaf rotated inward 78 degrees | Matching entry setting without a new camera angle |
| repaired-handrail | 1.80 m descent run; origin at top-approach height | Contrasting repair sleeve and two new screw heads |
| leaf-drain | 0.32×0.24 m grate; origin at supporting surface | Leaves caught across the grate |
| scraping-bin | 0.62 m body, loose lid pivot, brick beside it | Bin lid near the wall; future scrape and movement staging |

The bin stands beside the right boundary wall. Its loose lid reaches that wall, providing a physical basis for the later scrape. The bin lid and door leaf have separate pivots for later authored motion. Their current prefabs are static variants. The brick is beside the bin in this working staging; it does not establish when Jo last weighted the lid. No new chronology or factual inference is assigned to it.

The source camera housing has its lens at the recording viewpoint and a bracket attached to the boundary wall. The review scene adds concrete side banks, an upper approach, narrow boundary walls, and a brick foundation around the entrance. It reuses the exterior-camera prefab from the first Season 1 kit. Original chipped paint, stained concrete, muted leaves, and metal distinguish the entrance from the refuge's patched broadcast equipment.

## Fixed source coverage

Open `Assets/CommonHouse/Scenes/MotionDetectedSourceReview.unity`. `NetworkCamera` is the authored source viewpoint. `ArtistReviewCamera` is disabled by default and exists for asset inspection, not source footage. The scene stays outside playable build settings. To compare door poses in the Editor, expand `Source exterior only`, disable `basement-door-closed`, and enable `basement-door-open`. Use only one variant at a time.

| Source property | Working value |
|---|---|
| Lens position | x=-1.15, y=1.80, z=-0.85 m relative to the upper approach |
| Aim point | x=0, y=-0.75, z=1.40 m |
| Field of view | 68 degrees, vertical |
| Frame | 4:3; native captures are 960×720 |
| Bottom landing | y=-1.20 m |
| Door threshold | x=0, y=-1.20, z=3.04 m |
| Allowed observations | Exterior steps, drain, rail repair, bin lid, door and threshold |
| Excluded observations | Basement couch, bags, private actions, indoors, and movements after leaving coverage |

Both door variants retain a flat opaque coverage stop just beyond the frame. This is an exterior-source boundary, not a model of a furnished room. It prevents an open door from exposing invented indoor evidence. The set is an asset review, not an entrance to a playable interior.

MD-V01 will need rain, the scraping sound, and an indistinct pale edge without identifiable person detail. MD-V02 will need the son's clear appearance, damaged sleeve, and exact authored camera-addressed words. MD-V03 will need a continuous overlapping entry sequence. Those actors, sounds, captions, and media states are deferred. Keep one fixed source camera across all three clips; receipt order and asserted device time belong to the later reader, not an arbitrary texture date.

The set alone does not prove the son's identity, repair timing, unlocked condition, or present fate. Jo's recognition, her attributed account, the recorded action without an unlocking gesture, and explicit comparison remain separate requirements in the contract.

## Regeneration and checks

```sh
python3 scripts/build_motion_detected_assets.py
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.MotionDetectedSourceSetTests --graphics --timeout 600
python3 -m unittest discover -s tests -p test_build_season_assets.py
python3 scripts/check_provenance.py
python3 scripts/check_binary_sizes.py
python3 scripts/check_docs.py
```

The generator runs in a disposable licensed Editor snapshot. It exports only this kit and the source review scene. The shared narrow-export helper validates the allowlist and six-prefab count before copying anything. Existing Season 1 assets, outpost and intro scenes, Packages, and owner ProjectSettings are outside that export.

The native fixture checks saved assets and materials, door pivots, rail fasteners, the bin lid reaching the wall, each stair collision height, exterior clue framing, occlusion beyond the threshold, build exclusion, and six graphical source/detail views. Coverage checks and rendered asset inspection are distinct from animated clip validation and human interpretation of the episode.

See the [verification report and source renders](../verification/motion-detected-source-assets.md)
for observed local results and qualification limits.
