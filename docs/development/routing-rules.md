# Case 01 consent and routing rules

Issue [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8)
adds consent and send operations to the existing `CaseState`. It implements the
[causal contract](../planning/case-01-contract.md) without scene UI, persuasion
scores, new outcomes, or save files.

## Explicit authorization

`AcquireSource` and `ReplaySource` never grant authorization. Completing the
initial authored private conversation calls `RecordRefusalAuthorization` as a
separate event after E01 and E04. It records Ren’s authorized refusal. Evidence
collection alone, including C01, leaves authorization unknown.

`RecordPrivateContactAgreement` requires that initial authorization, E04,
both E06 accounts, and `InspectPrivateRoute`. It records a later explicit
agreement scoped to private contact. The route-inspection operation represents
completion of checking the diagram and actual connection; the domain cannot
prove that a future scene presents those interactions correctly.

Replaying the initial conversation cannot downgrade a later private agreement.
Repeated authorization events have no additional effects. The initial refusal
event ID cannot be reused for the later agreement. A player promise is separate
from Ren’s authorization.

## Draft, summary, and confirmation

`PrepareDraft` selects a response route without sending. Before E04, summaries
identify the unresolved recipient and all sends remain blocked. Cancelling
confirmation invalidates the presented summary while preserving the draft.
`Defer` also preserves draft, evidence, authorization, and missing source leads;
`Resume` permits a fresh summary. Defer is not an outcome.

`PresentSendSummary` returns the source, selected material, actual baseline
recipients, any blocking reason, and the unauthorized-disclosure warning.
Presentation code must display this exact summary before `ConfirmSend`.
The model requires explicit confirmation, but these domain tests do not prove
that a screen was rendered or read.

| Route | Send predicate | Selected material and recipients |
|---|---|---|
| R1 / private headset | E04, complete E06, verified private route, and current explicit private agreement | Connection metadata only; Ren and Ada. Private conversation text remains unknown to the player. |
| R2 / refusal relay | E04 and current refusal authorization | Only “Ren declines immediate contact” to Ada. The rest of C01, including identity and breach history, is not relayed. |
| R3 / room speaker | E04 and explicit acknowledgment of the unauthorized-disclosure warning | Ada’s request to the baseline room listeners: Ren, Ivo, and the player. Private agreement does not authorize this route. |

Every route also requires E01, an unresolved and resumed case, and explicit
confirmation. R2 requires no optional clues. R3 does not require the private
route inspection: its actual room recipients and warning are already explicit.
These recipient sets describe the fixed POC baseline, not dynamic occupancy
sensing. Scene integration must preserve or explicitly revise that contract.

Confirming checks the current authorization and eligibility again. A summary
from another session, a cancelled/replaced draft, or an earlier authorization
state is rejected without sending. In particular, an old refusal draft cannot
be sent after Ren agrees to private contact. The same successful send event
can be retried without changing outcome or disclosures; a second send event
cannot resolve the case again.

Outgoing disclosure records retain source attribution plus `MaterialId` and
`Text`. Consumers must use the selected material, not expand `SourceId` into the
entire original transcript. R1 records connection metadata rather than invented
private dialogue; R2 records only the authorized refusal. Acquired-source and
player-knowledge collections do not grow when a response is sent.

## Verification

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.RoutingRulesTests
```

The focused fixture covers each missing prerequisite, separate authorization,
minimal refusal, private recipients and knowledge boundaries, public warning
acknowledgment, stale summaries, cancellation, defer/resume, cross-session
summaries, and duplicate sends/aftermaths. The [route rehearsal](../planning/case-01-rehearsal.md) exercises the model
through both discovery orders, optional-content omission, and recovery. Scene interaction, desktop builds, and human
playtests are distinct evidence tiers. Native checks retain the isolation and
licensing boundaries in the [development guide](../development.md).
