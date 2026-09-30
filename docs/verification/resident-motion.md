# Resident motion verification

Verified locally on Kvant on 2026-09-29 for
[issue #85](https://git.subcult.tv/PatrickFanella/the-common-house/issues/85).
Source: `t3code/wire-resident-animations` in the thread worktree, with uncommitted
changes over `c786c0e4594ec688943b4725cae144b09fe623f6`. These are local results;
there is no hosted CI, published PR, or human playtest evidence for this change.

## Implemented behavior

The generated scene gives Ivo and Ren short standing, walking, sitting, and
return routines. Their interaction targets follow them. A resident plays a
standing or seated talking gesture while its authored response is displayed.
Closing, changing residents, and the existing state-load reset clear talking.
Modal screens hold travel, and residents yield to a nearby player. Ren stays
inside the archive alcove. Animation does not create case evidence or consent.

The importer extracts horizontal root motion instead of baking travel into
the skeleton. With `applyRootMotion` disabled, the resident body stays with
its scripted position and interaction target. Vertical posture remains in the
pose so seated clips lower the hips. Both Animators evaluate while offscreen.

## Checks

| Check | Result |
|---|---|
| Native full EditMode suite | 94 passed, zero failed or skipped |
| Native full graphical PlayMode suite | 97 passed, zero failed or skipped |
| Final graphical `ResidentAnimationTests` | 8 passed after refining capture timing and the camera view |
| Python suite in disposable T3 state | 64 passed |
| Asset provenance | 231 entries cover every sourced and LFS asset |
| Binary size check | 73 plain-Git binaries within the cap; 231 LFS files |
| Linux artifact and R2 player smoke | Verified after recovering the interrupted build launcher; see below |

The fixture checks actual humanoid clips, both route endpoints, seated hip
height, horizontal body alignment, following targets, standing and seated
responses, resident switching, Escape, load reset, modal pause, and yielding
to the player. The final capture-only refinement waits two frames for skinning
before rendering; it does not change resident runtime behavior.

Private evidence under `~/.local/state/common-house-unity/`:

- `editor-method-20260929T203907Z-rcn71w3t`: final scene and animation reimport.
- `tests-editmode-20260929T204112Z-eyl6_0gt`: 94 EditMode tests.
- `tests-playmode-20260929T204112Z-p3m10m2a`: full 97-test PlayMode suite.
- `tests-playmode-20260929T204431Z-4xnqlzef`: final focused suite and inspected PNGs for each resident's walking, seated, talking, and seated talking poses.
- `local-poc-build-20260929T204112Z-ycfj0mif`: final Linux artifact, output manifest, and R2 smoke.

The generated controller received trailing-whitespace cleanup after the full
suite snapshot; its serialized behavior is unchanged. Temporary native
projects and save directories were removed. Test containers and networks were
removed, and no document preview was started. Failed and superseded probes
retain their private evidence.

## Build launcher recovery

The final Python/RTK build launcher ended with status 143 during compilation.
Its owned Editor continued in the same disposable project. The original
Editor exit code is unavailable. Recovery waited for that process to finish,
required the successful batch completion marker, validated the Editor build
receipt and executable/data files, and hashed the artifact with the repository's
`build_poc.output_manifest` helper.

The existing `build_poc.run_player` helper then launched that exact artifact.
Its process exited successfully and the R2 receipt, screenshot, single outgoing
disclosure, 1280×720 screen size, isolated save identity, and unchanged artifact
manifest passed the same checks used by the build script. The receipt records
`launcher_interrupted: true` and the recovery. The interrupted launcher itself
is not reported as a successful end-to-end command.

Artifact manifest SHA-256:
`7d0b3455e10d7e1319a6162c57e87f90dce3f113014e8453d20831b012c9d207`.
The player remains at the build evidence directory's
`test-data/player/TheCommonHouse.x86_64`.

## Reproduce

```sh
rtk proxy python3 scripts/build_outpost.py
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ResidentAnimationTests --graphics --timeout 600
rtk proxy python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests --graphics --timeout 600
rtk proxy python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests --timeout 600
rtk proxy python3 scripts/build_poc.py --target local --smoke --timeout 900
```

These checks require the pinned Editor, normal local licensing, and the local
Vulkan display for graphical operations. They do not qualify Open Pilot Unity
execution or establish a human evaluation of the household motions.
