# Telecommunications infrastructure and printed graphics

[Issue #89](https://git.subcult.tv/PatrickFanella/the-common-house/issues/89)
extends the [office ground floor](office-complex.md) with original environmental
assets. `TelecomEnvironment.Build` runs from `OfficeComplex.Build`, before the
existing retro material application. `TelecomAssetKit` owns the reusable
geometry and deterministic print pixels; `OfficeAssetKit` supplies shared
texture import and bitmap lettering.

Elevated ladder trays carry three visible cable bundles from the service racks
through the concourse. Branches drop to junction boxes at the repair bench and
radio desk. A second service route reaches a wall-mounted cross-connect cabinet
with terminal blocks and copper-colored terminations. Two rack patch panels
have paired sockets and short lime/violet patch leads. These are presentation
props: their connections do not simulate electricity, radio routing, or the
incident's unresolved world rules.

Three original 128×192 prints apply the
[graphic reference study](../design/graphic-language.md): PATCH / SHARE uses a
connector and cable loop with lime; KEEP / LISTENING uses a terminal window and
pointers with violet; USE IT / PASS IT uses paired tools with violet. Halftone,
photocopy speckle, jagged paper seams, and diagonal accent strips are generated
from code. Paper tape is separate geometry. The prints occupy the repair room,
concourse, and commons; the sleeping bay has no new decoration or assigned
belongings. This is a provisional art study, not final character or story art.

All materials use `CommonHouse/Retro`; PNGs are point-filtered, uncompressed,
and have no mipmaps. Generated files remain under
`Assets/CommonHouse/Art/OfficeComplex/`. `build_outpost.py` records the telecom
and print textures with `TelecomAssetKit.cs` as their generator. No supplied
reference pixels or third-party marks enter these assets.

Regenerate before checking the actual scene:

```sh
python3 scripts/build_outpost.py
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.TelecomEnvironmentTests --graphics --timeout 600
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests --graphics --timeout 900
python3 scripts/build_poc.py --target local --smoke --timeout 900
```

The focused fixture checks equipment placement, overhead clearance, distinct
point-filtered prints, and the absence of new behavior components or evidence.
It requires graphics and captures five views for inspection. Existing office
tests exercise real player traversal and collision limits; the wider suite
checks Case 01 flows. The wall cabinet has collision, while cable trays, drops,
and paper dressing do not add movement obstacles or inspection targets.

Generation replaces `Assets/`, `Packages/`, and `ProjectSettings/` from its
disposable snapshot; finish source edits before starting it. Review the result
and remove unrelated serialization churn before running tests against the
final snapshot. Native checks use the existing Editor and normal license.

See the [verification report](../verification/telecom-environment.md) for
observed results, captures, and limits.
