# Modular city asset kit

[Issue #90](https://git.subcult.tv/PatrickFanella/the-common-house/issues/90)
implements the [city asset direction](../design/city-assets.md).
`CityAssetKit.Generate` saves 20 original prefab assets, shared retro materials,
and deterministic 64×64 textures under `Assets/CommonHouse/Art/CityKit/`.
`CityKitReview.Generate` assembles a separate review scene at
`Assets/CommonHouse/Scenes/CityKitReview.unity`.

## Using the kit

Drag prefabs from `Assets/CommonHouse/Art/CityKit/Prefabs/` into an authored
scene. Root scale is one; front is local -Z. Dimensions are in metres.

| Modules | Grid and pivot |
|---|---|
| Road straight/crossing | 8×8 m, surface at local y=0, origin at tile centre |
| Pavement | 4×8 m, walking surface at y=0.15 m, curb on local -X edge |
| Service alley | 4×8 m, surface at y=0, drain is presentation geometry |
| Facade bays | 4 m wide × 3 m tall; bottom-centre origin, wall extends into +Z |
| Roof cap | 4×4 m footprint; origin at roof level, parapet on -Z edge |
| Fire escape | 3.6 m landing; origin at attachment level, ladder extends 2.8 m downward |
| Furniture | Ground-level origin; shelter 3.4 m wide, bench 2 m wide, lamp 4.8 m tall, pole 6 m tall |

Rotate road/pavement tiles by 90 degrees for the review street's orientation.
Stack facade bays every 3 m and repeat sideways every 4 m. Place building roots
at pavement height when using the raised pavement. Facade doors/windows are
closed dressing on a solid wall, with opaque glass; they do not open into an
interior. Fire-escape traversal, curb accessibility, and navigation need a
separate check when a playable flashback is authored.

Prefabs use persistent built-in cube meshes, an original sign-face mesh with
explicit UVs, and stored materials, with simple
box collision on walking surfaces and solid equipment. Decorative slats,
windows, labels, and wires do not add colliders. The shared palette includes
original brick mortar, paving seams, asphalt grain, tinted glass, timber,
metal, paint, markings, and generic signs. Textures are point-filtered,
uncompressed, and have no mipmaps. No external artwork or city model is used.

## Generate and inspect

Use the existing licensed Editor and disposable runners:

```sh
python3 scripts/build_city_assets.py
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CityAssetKitTests --graphics --timeout 600
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests --graphics --timeout 900
```

The Common House / City Kit Editor menu also regenerates the assets and scene,
after prompting to save current work. The command-line generator copies only
Unity source directories back from a disposable snapshot. Finish source edits
before starting it; review the resulting diff afterward. The Python wrapper
records generated texture provenance and fails if the outpost scene or build
settings changed. Stage the generated PNGs before provenance/LFS checks.

The review scene combines reusable prefab instances and closed building
masses. It shares the existing retro volume, adds review-only ambient/fog
settings, and has a static camera. It is excluded from build settings.
No player, case session, narrative event, or flashback controller is added.

The focused fixture checks all prefab/material/mesh references, dimensions,
road seams with physical raycasts, scene isolation, texture imports, and four
rendered views. It requires graphics rather than silently skipping capture.
See [verification](../verification/city-assets.md) for observed evidence.
