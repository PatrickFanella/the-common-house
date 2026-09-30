# Arrival intro state and rehearsal

Issue [#94](https://git.subcult.tv/PatrickFanella/the-common-house/issues/94) implements IN-2 from the [intro handoff](../planning/intro-agent-handoff.md). Its prerequisite, [IN-1 content](intro-content.md), is locally implemented in the same worktree and remains unmerged. The [scene contract](../design/episodes/intro/case-contract.md) owns the intended learning and recovery behavior.

`CommonHouse.Domain.IntroState` is an isolated in-memory model. It takes validated `IntroContent`; it does not create a Case 01 session, write saves, start an episode, send transmissions, or execute cinematics. The model tracks acknowledged scenes, reviewed lines, complete acquired sources, selected expression options, optional preferred address, core acknowledgments, and completion.

## Explicit review boundary

`OpenScene`, `OpenSource`, and `OpenOrientation` return immutable `IntroReview` packets with speaker-labelled text, source descriptions, and available choices. They do not grant knowledge. Scene packets omit player-option lines and conditional response lines until `SelectOption` explicitly presents those responses. Previously presented answers remain readable within that packet; the latest selection for each menu is the pending expression. This is a text rehearsal, not timed delivery or a final dialogue layout.

`Acknowledge(eventId, review)` commits only the current presented packet. Source acquisition requires the complete transcript to have been explicitly acknowledged; partial answers can accumulate across visits without pretending an unasked question was answered. Document acquisition requires acknowledgment of its description. Selecting an expression or address does not commit it immediately. Name input remains a future presentation choice; the model accepts an explicitly supplied preferred address through IN-Q06-A, permits it to remain undecided, and supports leaving it blank through IN-Q06-B.

Pausing blocks selection and acknowledgment until resumed. `SkipToTranscript` marks the current text packet for transcript presentation; it grants no facts and does not choose skipped options. `Close` discards pending selections and returns to no active review. Reopening reconstructs the available content while retaining committed progress. Changing a selection creates a new packet; an old packet cannot be acknowledged in its place. An acknowledged event retry has no effect, and reusing its ID for another packet fails.

## Recovery and completion

`OpenOrientation` offers a readable recovery packet containing the shelter offer and the attributed warning, chronology, network explanation, recorded greeting, and Ivo's recognition. Opening it does not add facts; explicit acknowledgment does. It does not include the map or the four optional broadcast sources, and completion does not retroactively acquire them. The office greeting retains recorded delivery and unknown sender/date. The summary contains no player biography or explanation of the incident.

Completion requires the shelter offer, complete warning, complete network explanation, and recorded greeting to have been acknowledged. Acknowledging IN-S07 also requires an explicit IN-Q03 response. Both responses complete orientation. A blocked finish leaves facts and the pending review unchanged, so the player can close it, review missing content, and return. Scene order is flexible in the model; authored destination links do not gate essential recovery. After completion, sources and the desk's text remain reviewable. A case connector is separate future work.

## Focused verification

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroRehearsalTests
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroContentTests
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseStateTests
```

The rehearsal uses the actual resource for full orientation, both final responses, all shelter responses, minimal questions/privacy, flexible order, partial accounts, pause/skip/close recovery, optional address, event retries, replay, and rejected actions. These tests qualify the in-memory boundary. They do not establish a playable intro, camera or media behavior, save/reload, a desktop build, or human understanding. Use the existing disposable runner and normal local Unity license. See the [verification report](../verification/intro-state.md) for actual results.
