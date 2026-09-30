# Telecommunications environment verification

Local environment slice for [issue #89](https://git.subcult.tv/PatrickFanella/the-common-house/issues/89).
Source and regeneration details are in the [development guide](../development/telecom-environment.md).

## Scope

Original presentation props connect the utility racks, concourse, repair bench,
and radio area. Three original prints occupy the repair room, concourse, and
commons. The existing scene targets, case content, and resident routines are
preserved. The corrected focused scene fixture passed all three tests, and the full
EditMode suite passed all 94 tests. All 112 graphical PlayMode tests also passed. The fresh Linux build and R2 player smoke also passed. The initial focused run caught
Unity resizing a 128×192 print to 128×256; the importer now explicitly keeps
non-power-of-two dimensions. Its failed receipt is retained, not counted as
a pass. The initial five captures were inspected before this correction.

## Evidence boundary

Native scene tests, inspected captures, compiled Linux smoke, and human playtest
are separate evidence. Automated success cannot establish comfort, atmosphere,
or comprehension. No human playtest, hosted CI, publication, network simulation,
upper-floor interior, or incident-rule implementation is claimed by this slice.

## Portable checks

The isolated T3 test snapshot passed 64 Python regression tests and document
validation (75 Markdown documents, 261 local links). Its test containers and
test network were removed by the launcher. Asset checks passed with 276
provenance entries and 276 LFS files; 73 plain-Git binaries were below the
5 MiB cap. All nine added PNGs have generated provenance. These checks do not
prove Unity rendering or scene behavior.

## Native focused evidence

Editor: Unity 6000.4.10f1 on Kvant, using the normal local license and Vulkan
display. Final generation: `editor-method-20260930T002609Z-gojgbaxf`.
Unrelated resident-controller and retro-volume serialization churn was restored
to the committed baseline before tests.

- Focused graphical PlayMode: `tests-playmode-20260930T002805Z-c7g_r0cz`,
  3 passed, 0 failed/skipped. All five captures inspected: utility patching,
  line frame, concourse, repair print, and commons print.
- Full graphical PlayMode: `tests-playmode-20260930T003003Z-6ivamf5i`,
  112 passed, 0 failed/skipped. Includes the existing real-player office
  traversal and Case 01 routes; all five final telecom captures inspected.
- Full EditMode: `tests-editmode-20260930T002815Z-_a9556_j`,
  94 passed, 0 failed/skipped.
- Initial focused failure: `tests-playmode-20260930T002343Z-r8tw8m9u`,
  2 passed and 1 failed because imported poster height was 256 rather than 192.
  Corrected with explicit `TextureImporterNPOTScale.None`; the final fixture
  checks the actual loaded dimensions.

Receipts and PNGs remain under `~/.local/state/common-house-unity/`; disposable
project and save directories are removed by the runners.

## Linux artifact

`local-poc-build-20260930T003003Z-pfzbvt1p` built and launched the Linux player
at 1280×720. R2 smoke passed with one outgoing disclosure; the aftermath
screenshot was inspected. The build-output manifest digest is
`a92d0025ff9b7536f2dd0c2f47c37c6b606c3c1e639f2f35dd2c31f065cf1624`.
The retained player is
`~/.local/state/common-house-unity/local-poc-build-20260930T003003Z-pfzbvt1p/test-data/player/TheCommonHouse.x86_64`.
Its file manifest was recomputed after smoke and matches the retained receipt.

All 1,241 Unity input hashes in the focused, full PlayMode, EditMode, and build
receipts match the final checkout. Project and disposable save directories are
removed; logs, captures, receipts, and the compiled artifact remain.
