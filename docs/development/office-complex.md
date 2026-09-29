# Converted-office ground floor

[Issue #87](https://git.subcult.tv/PatrickFanella/the-common-house/issues/87)
implements the first slice of the [home-base direction](../design/home-base.md).
`OfficeComplex.Build` extends the existing outpost generator. Case room positions,
anchors, targets, resident routes, and the first-person spawn stay in place.

The old south wall is replaced by three doorways. A four-metre-wide concourse
connects the existing rooms to the reception commons, repair hackerspace,
communal sleeping bay, and utility room. The ground floor occupies x=-14..14,
z=-16..4, with ceilings at three metres. `OutpostLayout.PresentDayBounds` covers
that floor. Its original three traversal points remain the Case 01 route.

`OfficeAssetKit` builds original reusable workbenches, terminals, equipment
racks, cots, lockers, sofas, toolboards, cable runs, and graphic signs. The
small procedural textures, bitmap lettering, and original broadcast-ring graphics live in
`Assets/CommonHouse/Art/OfficeComplex/`. Materials use `CommonHouse/Retro`.
The generator retains office ceiling grids, partitions, curtain-wall framing,
and carpet alongside the household additions. The larger exterior shell uses
the same renderer; retro fog runs from 10 to 85 metres with a cool gray color to keep its massing visible.
Brighter exterior material colors stand in for sunlight in the vertex shader;
this is an art treatment, not qualified exterior lighting or GI.

The window treatment is opaque faux glazing within the existing retro shader.
It has no physical glass, reflection probes, or destructibility. Upper floors
are exterior massing only. Closed ground-floor boundaries prevent exterior
travel, and there is no stair or lift route yet. Batteries and terminals are
presentation props, not simulations or new interactions.

Regenerate and verify in disposable native state:

```sh
python3 scripts/build_outpost.py
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.OfficeComplexTests --graphics --timeout 600
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests --graphics --timeout 900
python3 scripts/build_poc.py --target local --smoke --timeout 900
```

`build_outpost.py` records each generated office PNG in the provenance manifest.
Stage new binary files before running provenance and size checks so the
checkers can inspect their tracked paths and LFS pointers. Use the existing
normal Editor license; the runner does not qualify Open Pilot Unity support.

The focused fixture walks the real player through the new rooms, checks front
and side collision limits, verifies stable case anchors and empty evidence,
and captures the corridor, workshop, commons, sleeping bay, utility room, and
exterior shell. Automated traversal and rendered review are distinct from a
human assessment of navigation, atmosphere, or comfort.

See the [local verification report](../verification/office-complex.md) for
native counts, inspected captures, artifact evidence, and limitations.
