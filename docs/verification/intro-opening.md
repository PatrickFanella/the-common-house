# Arrival opening verification

Issue [#95](https://git.subcult.tv/PatrickFanella/the-common-house/issues/95), IN-3. Local work on Kvant, 2026-09-29 America/Chicago, in `/home/onnwee/.t3/worktrees/the-common-house/t3code-7e11d389`. The prerequisite commit is `812c2a3`; opening implementation was dirty during verification. Unity is the actual locally licensed 6000.4.10f1 Editor. Native prerequisite issues #92 and #94 remain open/unmerged; no automation was queued.

## Scene generation

`python3 scripts/run_editor_method.py CommonHouse.Editor.IntroOpeningSceneGenerator.GenerateAndExport` passed. Final receipt: `/home/onnwee/.local/state/common-house-unity/editor-method-20260930T022924Z-60ss4i9x`. The disposable Editor generated a real saved scene. Only `IntroOpening.unity`, its metadata, and `EditorBuildSettings.asset` were copied back. Existing build entries were preserved and the arrival entry appended. Empty-value trailing spaces in the new scene/metadata were normalized before the final scene tests. No Case 01 scene, material, character, save, or session was replaced.

## Actual scene behavior

`python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.IntroOpeningTests` passed ten tests on the final scene/controller implementation. Receipt: `/home/onnwee/.local/state/common-house-unity/tests-playmode-20260930T023115Z-a0gyvvp_`.

The tests load the saved scene and exercise focused E interactions, range/occlusion, keyboard option/acknowledgment routes, mouse action dispatch, all shelter responses, optional address/privacy, need-a-minute recovery, Escape, pause/transcript/resume, modal competition, repeated warning review, and movement through the doorway. The starting view focuses the entrance notice. Plain pause lists only available intro controls. Modal conversation does not change the prepared camera pose. No CaseSession exists in the intro scene, and no greeting, network, or full-intro completion is fabricated by this slice.

The existing `OpeningSceneTests` regression passed five PlayMode tests. Receipt: `/home/onnwee/.local/state/common-house-unity/tests-playmode-20260930T022247Z-fl6zml_z`. The new Editor capture helper was present in this run's source snapshot and compiled successfully.

The capture launcher's final focused standard-library regression passed four tests through the isolated T3 recipe. It checks all seven intro capture requirements, missing-artifact failure and cleanup, preserved baseline guidance capture, and conflicting capture selections. Command: `rtk proxy ~/.local/bin/t3-dev --profile the-common-house test --command 'python3 -m unittest discover -s tests -p test_capture_panels.py'`. These mocked launcher tests are not graphical evidence.

The first graphical run passed at `/home/onnwee/.local/state/common-house-unity/panel-graphics-20260930T022551Z-ler31tsc`. Image inspection exposed authoring notes in scene descriptions; code inspection found Case 01-specific plain-pause help. The final implementation uses player-facing context, readable source titles, and an intro-specific pause overlay. All 66 dialogue lines and all authored data other than the eight scene descriptions were compared against `812c2a3` and remain unchanged.

The affected `IntroContentTests` regression passed 31 EditMode tests after the description edits. Receipt: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T023320Z-wskl_yqp`.

Document validation passed 86 nonempty Markdown files and 310 local links. Whitespace checks passed for tracked changes and new source, scene, metadata, test, and documentation files.

## Rendered inspection

`python3 scripts/capture_panels.py --intro --scale 1.5` passed with the final capture helper. Receipt: `/home/onnwee/.local/state/common-house-unity/panel-graphics-20260930T023929Z-ltmqn4ds`. All seven actual Game-view images were inspected at 800×600 and 1.5× text scale: world/entrance prompt, selected arrival response, warning text, warning actions, welcome text, welcome actions, and plain pause.

The entrance prompt is visible from the starting view. Player-facing context and readable source titles replace author notes and internal IDs. Long text remains inside a scrollable panel, and the scrolled action captures show readable choices, acknowledgment, pause, transcript, close, and keyboard help. The plain-pause panel lists intro controls without advertising Case 01 save/notebook/hint features. Geometry is visibly primitive staging, not final art. The capture helper explicitly refreshes focus in its test context; normal play uses the existing native-input Update path.

## Evidence limits

Scene tests use deterministic input frames, not host keyboard/mouse events or a human participant. Rendered capture establishes what the Editor actually displays; it does not measure comprehension or comfort. There is no desktop build, save/reload, full intro, hosted CI, or Open Pilot worker qualification in IN-3. Raw logs and graphical receipts remain privately on Kvant; final art and actor performance are not claimed.
