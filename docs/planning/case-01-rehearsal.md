# Case 01 text-only route rehearsal

Issue [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9)
rehearses the [causal contract](case-01-contract.md) through the real
`CaseContent` and `CaseState` implementations. Names, excerpts, and the shared
repair detail remain provisional. There is no second runtime or scene UI.

## Executable coverage

Run from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseRehearsalTests
```

The fixture contains eleven runs:

- Six runs combine R1/R2/R3 with archive-first and conversation-first discovery.
  Every acquired source is read as text, replayed, and delivered again under both
  the original and a fresh event ID. E02 includes its bell caption. Both orders
  reach the same outcome-specific recipients and a single aftermath completion.
- Three runs skip E02/E03/E05, cancel a pre-identity draft, defer, resume, reach
  the private question, then cancel/defer/resume again before completing each
  outcome. R2 also skips E06 and still completes.
- Two runs acquire only one E06 account in either order, reject premature private
  agreement, retain authorized refusal, defer/resume, recover the other account,
  and complete R1.

Every run uses the public routing API and asserts its expected outcome and
recipients independently of the generated send summary. No test calls internal
resolution storage. Sending must leave player knowledge unchanged; repeated
confirmation and aftermath delivery cannot add disclosures or effects.

## Observed action transcripts

These excerpts are from the passing focused run on Kvant with Unity
6000.4.10f1 on September 24, 2026 (September 25 UTC). Each comes from
`SkipOptionalContentCancelDeferAndResumeStillReachEveryOutcome` with the named
route. Per-test NUnit output in `results.xml` includes the complete action
trace and final facts. The long serialized final-facts lines are summarized in
the table below rather than duplicated here.

The native receipt is retained privately at
`~/.local/state/common-house-unity/tests-editmode-20260925T033404Z-tueatzo2/`.
The excerpts below are test output, not raw Editor/licensing logs.

### R1 — private contact

```text
Opening household care is an authored scene prerequisite, not simulated here.
Read/replay T01 [ada]: Ada: I am Ada. Please tell Mara that the road is clear. I can remain available for a later response.
Complete evidence: E01
Skip E02/E03/E05. Cancel pre-identity draft and defer; private Ren question remains available.
Read/replay C01 [ren]: Ren: Mara was my network name. My location was relayed without my permission. I do not want immediate contact; you may relay my refusal.
Complete evidence: E01, E04
Ren explicitly authorizes refusal; this is separate from acquiring E04.
Resume with draft, refusal authorization, and both follow-up leads intact.
Read/replay T03 [ada]: Ada describes a mundane repair shared with Mara at the house. [Specific repair detail remains provisional.]
Complete evidence: E01, E04
Read/replay C02 [ren]: Ren corroborates the shared repair detail described by Ada. [Specific repair detail remains provisional.]
Complete evidence: E01, E04, E06
Both repair accounts complete E06. Inspect diagram and actual private connection; no new consent yet.
Ren explicitly agrees to private contact in the later conversation.
Present R1: Establish private contact; the conversation contents remain private.
Source T01; recipients: ren, ada
Aftermath phase recorded once. The matching household scene is not implemented by this rehearsal.
```

### R2 — refusal relayed

```text
Opening household care is an authored scene prerequisite, not simulated here.
Read/replay T01 [ada]: Ada: I am Ada. Please tell Mara that the road is clear. I can remain available for a later response.
Complete evidence: E01
Skip E02/E03/E05. Cancel pre-identity draft and defer; private Ren question remains available.
Read/replay C01 [ren]: Ren: Mara was my network name. My location was relayed without my permission. I do not want immediate contact; you may relay my refusal.
Complete evidence: E01, E04
Ren explicitly authorizes refusal; this is separate from acquiring E04.
Resume with draft, refusal authorization, and both follow-up leads intact.
Present R2: Ren declines immediate contact.
Source C01; recipients: ada
Aftermath phase recorded once. The matching household scene is not implemented by this rehearsal.
```

### R3 — informed public disclosure

```text
Opening household care is an authored scene prerequisite, not simulated here.
Read/replay T01 [ada]: Ada: I am Ada. Please tell Mara that the road is clear. I can remain available for a later response.
Complete evidence: E01
Skip E02/E03/E05. Cancel pre-identity draft and defer; private Ren question remains available.
Read/replay C01 [ren]: Ren: Mara was my network name. My location was relayed without my permission. I do not want immediate contact; you may relay my refusal.
Complete evidence: E01, E04
Ren explicitly authorizes refusal; this is separate from acquiring E04.
Resume with draft, refusal authorization, and both follow-up leads intact.
Present R3: Ada requests Mara: the road is clear.
Source T01; recipients: ren, ivo, player
Acknowledge warning: This room-speaker disclosure is not authorized.
Aftermath phase recorded once. The matching household scene is not implemented by this rehearsal.
Saving results to: /home/onnwee/.local/state/common-house-unity/tests-editmode-20260925T033404Z-tueatzo2/results.xml
```

## Final facts and aftermath boundary

| Route | Required authorization/facts | Observed final facts | Authored household aftermath, not rendered here |
|---|---|---|---|
| R1 | E01/E04, both E06 accounts, private-route inspection, later explicit agreement | R1; `AftermathCompleted`; private-contact authorization; connection metadata to Ren/Ada only; player knowledge unchanged | Ren invites the player into a routine repair and acknowledges the boundary. |
| R2 | E01/E04 and current explicit refusal authorization | R2; `AftermathCompleted`; only the refusal sent to Ada; E02/E03/E05/E06 remain unacquired in the minimal run | Ren remains available and appreciative; the case is complete. |
| R3 | E01/E04, identified room recipients, explicit confirmation and warning acknowledgment | R3; `AftermathCompleted`; request disclosed to Ren/Ivo/player; private contact was not authorized in this run | Ren asks for distance; later acknowledgment and a repair attempt do not erase the breach. |

Refusal is a completed outcome. Defer never produces an ending. Optional clues
can add context but do not remove the private-question or refusal route. A
single repair account leaves E06 incomplete and cannot authorize contact.

## Contradictions and limits

No causal contradiction was observed in these eleven executions. Missing
prerequisites intentionally produce rejection, and assertions require those
rejections; they are not relabeled as successful routes. Failures in future
runs must retain their XML/log evidence and be resolved or reported explicitly.

Reachability here means the real model retains missing sources and accepts the
subsequent valid actions. It does not establish that a player can find a room,
resident, object, or dialogue option in a scene. The opening act of household
care and the outcome-specific domestic scenes remain authored expectations.
`AftermathCompleted` proves the state transition, not rendered scene content.

The transcripts use provisional source text; their existence does not finalize
repair details or prove emotional impact, mystery comprehension, or pacing.
Save/reload, graphical interaction, keyboard navigation, desktop builds, and
human playtests remain later verification work. Local Editor results do not
qualify Open Pilot worker execution. See the
[development guide](../development.md) for native licensing, isolation, and
cleanup boundaries.
