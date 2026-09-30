# Intro persistence: next bounded unit

[Intro handoff](intro-agent-handoff.md) · [Presentation guide](../development/intro-opening.md). IN-4a supplies all eight authored scenes and explicit in-session completion. IN-4b must add an isolated reload contract before a runtime Episode 1 connector is implemented. This file plans that unit; it does not implement a save or queue automation.

## One outcome

Preserve explicitly acknowledged intro progress across restart without inventing pending selections, repeating a fictional outgoing exchange, or touching Case 01 saves.

Read the native IN-4a issue [#101](https://git.subcult.tv/PatrickFanella/the-common-house/issues/101), its dependency #95, current controller/state, and the actual persistence seams before opening an implementation issue. A passing presentation test is not evidence that reload works.

## State ownership

`IntroState` owns committed reviewed line IDs, acquired source IDs, acknowledged scenes, expressions, optional preferred address, and completion. Its active review, pause state, unacknowledged choice responses, and temporary UI selection are presentation state. A save contains committed state only. Loading closes any pending panel after the new snapshot validates; a rejected load leaves the existing state and modal owner intact.

Use a separately versioned intro snapshot with content ID/version and a separate file name. Check all IDs against the actual IntroContent. Reject duplicate fields/IDs, unknown sources or choices, invalid option ownership, control characters in an address, inconsistent source acquisition, and completion without the core acknowledgments and final response. A valid completed save reloads completed without playing another greeting, sending an arrival statement, or starting a case.

Do not assume CaseState's six-clue and three-outcome invariants apply here. Reuse file-writing infrastructure only after checking its content coupling. An intro file must not be accepted as Case 01 data or vice versa.

## Storage and failure behavior

Use an explicit development/test save root and an intro-specific path; do not silently select an ordinary user slot. Tests use unique disposable roots. Preserve an invalid primary and any backup for inspection. Validate a candidate snapshot before changing live state. Use atomic replacement, retain the previous good backup, and distinguish backup recovery from normal load. No new external services or credential handling.

Persist the fact that the radio exchange was acknowledged so replay stays labelled local. The prototype runs authored text; saving cannot imply a real transmission was performed. Completion remains a settled-in state, not a shift contract or episode start.

## Player controls

Choose and document explicit Save/Load controls consistent with the existing POC controls. Only advertise them when an intro store is configured. The intro pause help currently omits Save, Load, notebook, and hints deliberately. Update it only after implementing the actual controls. A missing store produces a readable unavailable message and creates no file.

Load should require a deliberate action. Never load silently on entering the scene or while the user is confirming another action. Explain that unacknowledged choices are not saved. Avoid requiring a preferred address to save or finish.

## Focused fixtures to implement

| Fixture | Meaningful coverage |
|---|---|
| EditMode `CommonHouse.Tests.IntroSnapshotTests` | Valid round-trip, missing/duplicate/foreign IDs, inconsistent completion, choice ownership, optional address, no pending facts |
| EditMode `CommonHouse.Tests.IntroSaveStoreTests` | Primary/backup recovery, invalid-primary preservation, separate file identity, failed-load state isolation, atomic write cleanup |
| PlayMode `CommonHouse.Tests.IntroReloadTests` | Actual saved scene, explicit controls, paused/pending review, pre/post-completion reload, local radio replay, no CaseSession or episode start |

These fixtures do not exist yet. Their commands must be made meaningful by the scoped implementation. Then use the actual pinned/licensed Editor through `scripts/run_unity_tests.py`, with the appropriate platform and fully qualified fixture. Missing results or zero tests fail. Regress IntroOpeningTests and existing baseline persistence only if a shared storage seam changes.

## Exclusions and evidence

No MD runtime, automatic next episode, save migration from a nonexistent intro format, final assets, actor work, secret incident reveal, broad framework extraction, or runner-policy change. Return source revision, focused counts, file identity and recovery proof, and remaining human observation. A local reload test does not qualify a new desktop build or hosted worker.
