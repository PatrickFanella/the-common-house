# Intro state verification

Issue [#94](https://git.subcult.tv/PatrickFanella/the-common-house/issues/94), IN-2. Local qualification on Kvant, 2026-09-29 America/Chicago, in `/home/onnwee/.t3/worktrees/the-common-house/t3code-7e11d389`, using Unity 6000.4.10f1. Base revision is `f4d1ab27c67bcdb88608263abb20ac7aeeeea6f7`; the intro content prerequisite, authoring documents, and new state implementation are uncommitted work. Native dependency #92 remains open. No review or merge is implied by local tests.

## Implemented boundary

`IntroState` rehearses explicit review, optional expression/address, partial source acquisition, core acknowledgment, recovery, and completion against the authored Intro resource. Reviews expose labelled text and descriptions. Opening, pausing, skipping, choosing, and closing do not silently apply learning. The model has no Case 01 state/session, persistence, outgoing transport, scene, or cinematic integration.

## Focused results

The final `IntroRehearsalTests` run passed 22 EditMode tests. Command: `python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroRehearsalTests`. Receipt directory: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T020421Z-e5ikt_os`.

This includes full and minimal routes, both closings, all three shelter responses, partial optional accounts, flexible order, interruption recovery, explicit preferred address, retries/stale reviews, replay, and each core requirement independently blocking completion. The earlier 17-test run passed before the final five cases were added.

The existing `IntroContentTests` regression passed 31 EditMode tests. Command: `python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroContentTests`. Receipt directory: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T020601Z-h4m4_zv4`.

The existing `CaseStateTests` regression passed 18 EditMode tests. Command: `python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseStateTests`. Receipt directory: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T020733Z-88feaiu4`. All three focused commands exited zero with nonzero validated results; missing results and zero matching tests fail through the repository runner.

Document validation passed with 84 nonempty Markdown documents and 300 local file links. Tracked changes and the new state, rehearsal, and documentation files passed whitespace checks; both scripts have Unity metadata.

The existing runner uses the local licensed Editor in a disposable project with unique save identity. Raw logs and receipts stay privately on Kvant. This evidence does not qualify scene presentation, save/reload, a player build, human orientation, hosted CI, or an unattended Open Pilot worker.
