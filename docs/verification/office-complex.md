# Converted-office home base verification

Local verification on Kvant, 2026-09-29, for
[issue #87](https://git.subcult.tv/PatrickFanella/the-common-house/issues/87).
Source: `t3code/wire-resident-animations` in the thread worktree, with the
office conversion over `9ac9a3d0be55edfb15ebc03ca95b560b6b39004e`.
The [design direction](../design/home-base.md) records the owner-selected office
reference and DedSec influence. These are local checks, not hosted CI or a
human evaluation of atmosphere or navigation.

## Implemented slice

The existing three Case 01 rooms connect to a former office concourse,
reception commons, repair hackerspace, communal sleeping bay, and utility
room. The new floor is physically bounded and has a continuous route back to
the case rooms. The archive doorway was shifted to avoid the existing tape
target; that target and the other case anchors stay in place.

The exterior is a multi-storey massing study with pale wings, curtain-wall
bands, entrance recesses, repeated side fins and window slots, and a radio
mast. The ground floor retains suspended ceilings, partitions, carpet, door
frames, and faux glazing. Original procedural workbenches, terminals,
equipment racks, tool storage, sofas, reception furniture, unassigned cots,
lockers, battery cases, signs, and broadcast-ring graphics furnish the
conversion. The asset pipeline adds 36 generated office textures.

Upper floors have no playable interiors, stairs, or lift. Exterior ground is
visual context only. Faux glazing is opaque, and brighter exterior material
colors stand in for sunlight in the existing retro shader. This slice does
not qualify physical exterior lighting or GI. The locations remain
provisional; no resident identities, stories, case rules, or evidence changed.

## Native scene checks

The focused graphical `OfficeComplexTests` passed all five tests after
correcting the archive entrance. The final full graphical PlayMode suite
passed all 109 tests, with zero failures or skips. It includes the final
graphic, facade, and capture refinements. Its Unity source hashes match the
current assets, packages, and project settings.

The final EditMode suite passed all 94 tests, with zero failures or skips.
Its source hashes also match the current Unity inputs.

The focused checks walk the real `FirstPersonMovement`/`CharacterController`
through every new destination and back through the Case 01 rooms, without
teleporting along the route. They test front and side collision boundaries,
floor continuity, named spaces, original prop/material presence, stable case
anchors, and empty case evidence after walking. Full PlayMode includes the
existing case routes, save/load, privacy and consent, modal controls, and
resident motion/facing checks.

All six final PNGs were inspected: exterior, concourse, repair hackerspace,
reception commons, sleeping bay, and utilities. The final workshop view shows
the terminals and toolboard facing into the room and the layered original
print. The commons view shows the reused reception counter, seating, and
broadcast-ring graphic. These are scripted native renders, not a human
playtest.

Private evidence under `~/.local/state/common-house-unity/`:

- `editor-method-20260929T221208Z-prd7za8n`: final scene generation.
- `tests-playmode-20260929T220851Z-hr0r9utr`: focused five-test pass before the final graphic and side-facade refinement.
- `tests-playmode-20260929T221339Z-oab1b56c`: final 109-test suite and inspected world captures.
- `tests-editmode-20260929T221647Z-ly6c655e`: final 94-test EditMode suite.
- `local-poc-build-20260929T221341Z-z397gdzw`: final Linux artifact and R2 player smoke.

The first focused probe retained its failed result: a straight archive return
was blocked by the existing tape target. The revised doorway and final real
controller traversal resolve that reported failure. Unrelated regeneration
churn in the resident controller and retro volume profile was restored before
the final scene checks.

## Portable checks

The Python suite passed all 64 tests in disposable T3 state after the office
provenance helper was added. Provenance validation covers all 267 asset entries.
Binary validation passed for 73 plain-Git binaries within the 5 MiB cap and
267 LFS files. New procedural PNGs use LFS and name `OfficeAssetKit.cs` as their
generator. No new external assets or licenses were added.
Documentation validation passed for 70 Markdown documents and 231 local links;
the staged diff has no whitespace errors.

## Linux artifact and cleanup

The normal local build command passed with Editor `6000.4.10f1`. Its exact
Linux artifact then completed the R2 smoke route with one outgoing disclosure
and a 1280×720 screenshot. The artifact manifest stayed unchanged across the
smoke run. The build's source hashes match the final Unity inputs.

Artifact manifest SHA-256:
`490d34df01b1abed80f4ab17986c5e291a479d16cbfd606e073845487e6b80a4`.
The executable is retained in the build evidence directory at
`test-data/player/TheCommonHouse.x86_64`.

The R2 smoke validates the built case flow. World traversal and appearance
are supported separately by the native scene tests and inspected captures.
No human navigation or atmosphere assessment is claimed.

Disposable native projects and build save state were removed. No task-owned
test containers or networks remain, and no document preview was started.
Private receipts, failed probes, captures, and the playable artifact remain.

## Reproduce

```sh
rtk proxy python3 scripts/build_outpost.py
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.OfficeComplexTests --graphics --timeout 600
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests --graphics --timeout 900
rtk proxy python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests --timeout 900
rtk proxy python3 scripts/build_poc.py --target local --smoke --timeout 900
```

Use the pinned Editor and normal local licensing. Local native success does
not qualify an unattended Open Pilot Unity worker or establish publication.
