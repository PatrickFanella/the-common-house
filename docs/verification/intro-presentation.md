# Complete intro presentation verification

Scope: IN-4a, [issue #101](https://git.subcult.tv/PatrickFanella/the-common-house/issues/101). This extends the previously merged opening in PR #96. The content asset and IntroState remain unchanged. The [guide](../development/intro-opening.md) describes playing the scene, and [IN-4b](../planning/intro-persistence-handoff.md) owns the next persistence boundary.

## Result

All eight authored intro scenes are reachable in the saved IntroOpening scene. Four targets add radio programs, the recorded greeting, source-backed orientation, and tomorrow's offer. The same controller and readable UI handle both final responses, recoverable early completion, and local replay. Completion starts no case, assigns no name, and writes no save.

The scene generator was run in a disposable Editor project. Only the exported IntroOpening scene was copied back; metadata and the build list were unchanged. Scene serialization was normalized for trailing whitespace. No baseline scene, material, content asset, character, or owner-generated project setting was copied back.

## Actual checks

| Check | Result | Private receipt |
|---|---|---|
| Scene generation | Passed with pinned Unity 6000.4.10f1 and normal licensing | `editor-method-20260930T042332Z-jdv_lmhh` |
| IntroOpeningTests | 14 PlayMode tests passed | `tests-playmode-20260930T042513Z-na15zxq_` |
| Existing OpeningSceneTests | 5 PlayMode tests passed | `tests-playmode-20260930T042806Z-4_yfmc2x` |
| Capture launcher | 4 Python tests passed in isolated T3 test environment | Explicit `test_capture_panels.py` command |
| Rendered presentation | 16 actual Game-view images captured and inspected at 800×600, text scale 1.5 | `panel-graphics-20260930T042804Z-9w60typq` |

Receipts live under `/home/onnwee/.local/state/common-house-unity/`. The graphical check uses explicit focused-target positioning and action calls, not native human input. It checks the real rendered panels and their scrolling, not a human's understanding of the story. The final wording-only run also passed with all 16 required images at `panel-graphics-20260930T043213Z-85hhbs2x`. Its completion image was inspected separately and shows the final player-facing message, "You have a place here. The desk can wait until you are ready." The other panel layouts were unchanged from the fully inspected first run.

The added behavioral checks cover both final responses, a complete transcript-oriented route, missing-acknowledgment messages, no learning on a failed finish, recovery through the summary, an explicitly required final response, local radio replay, greeting cancellation, source delivery type, and no CaseSession/case start. The original range, occlusion, walking, modal ownership, camera-control, pause, privacy, and shelter checks remain in the 14-test fixture.

## Evidence limits

The snapshot included current owner-generated Editor settings, preserved outside the commit. Focused behavior passed before a subsequent wording-only completion-message change; the final graphical run also imports/compiles that wording. No state or control behavior changed after the passing tests.

Intro persistence, actual audio performances, final actors/geometry, a runtime Episode 1 connector, a new player build, hosted Unity CI, and a human completion playtest remain unqualified. Full text-first presentation is implemented; that is not a claim that IN-4b or every season episode is implemented.
