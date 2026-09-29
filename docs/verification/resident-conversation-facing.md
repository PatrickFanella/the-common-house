# Resident conversation facing verification

Verified locally on Kvant on 2026-09-29 for
[issue #86](https://git.subcult.tv/PatrickFanella/the-common-house/issues/86).
The source is the thread worktree on `t3code/wire-resident-animations`, with
the facing changes over animation commit `b42831828724c3e7c78a9df702bc4908276fe7e5`.
These results cover local native tests and rendered poses. They do not establish
hosted CI, publication, or a human playtest.

## Behavior and focused checks

The active resident faces the player's camera while listening or displaying
a response. Upright residents gradually turn their body. Seated residents keep
their body and chair alignment and turn their head within 65 degrees of yaw and
20 degrees of pitch. Head rotation runs after animation and uses an absolute
rig reference to avoid accumulating turns while a listening clip is frozen.

Closing, switching residents, and the existing load reset release facing.
Standing residents return toward their prior orientation; resumed walking
takes control of its route direction. Escape pause holds orientation. Sitting
and standing transitions keep their existing orientation. Facing does not move
residents or targets and does not grant evidence or consent.

The focused graphical `ResidentConversationFacingTests` suite passed all seven
tests, with zero failed or skipped. Tests exercise actual humanoid head bones,
gradual body turns before the first response, inactive residents, seated bounds,
repeated updates on a frozen listening pose, switching, Escape, load reset,
interrupted walking, pause, and sitting transitions.

Evidence directory under `~/.local/state/common-house-unity/`:
`tests-playmode-20260929T213036Z-d_2fmhi4`.
Its four PNGs show Ivo and Ren standing and seated while facing the camera.
All four were inspected. Captures wait two frames for skinning before rendering.

The full graphical PlayMode suite passed all 104 tests, with zero failed or
skipped, in `tests-playmode-20260929T213425Z-34ahfqyn`. Its source hashes match
the current Unity assets, packages, and project settings. This run includes the
existing animation, movement, dialogue, modal, save, and case-flow checks.
Documentation validation passed for 67 Markdown documents and 221 local links.

## Linux build

The normal local build command passed, followed by the R2 smoke run of that
exact Linux artifact. Evidence is in
`local-poc-build-20260929T213418Z-b4g6mjv7`. The receipt records Editor
`6000.4.10f1`, a successful R2 route with one outgoing disclosure, and a
1280×720 screenshot. The source hashes match the current Unity inputs, and the
artifact manifest stayed unchanged across the smoke run.

Artifact manifest SHA-256:
`14e653ea6ece2035f1eebddcdca8c30a07940cd6f6c159e5c8e9cd6b148f1181`.
The executable remains at the evidence directory's
`test-data/player/TheCommonHouse.x86_64`. Disposable projects and save state
were removed. This smoke check validates the built case flow; the focused
native tests and inspected captures provide the conversation-facing evidence.

## Reproduce

```sh
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ResidentConversationFacingTests --graphics --timeout 600
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests --graphics --timeout 900
rtk proxy python3 scripts/build_poc.py --target local --smoke --timeout 900
```

Use the pinned Editor and normal local licensing. The scripts isolate native
projects and save state. No scene regeneration, new animation clips, or package
changes are required for this slice.
