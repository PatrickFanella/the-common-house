# Case 01 in-memory state

Issue [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7)
adds `CommonHouse.Domain.CaseState`, separate from the
[authored content](case-content.md) and scene components. Its assembly has no
Unity engine references. It uses no clock, random input, storage, or services.
Each session receives its own state while sharing read-only case content.

`AcquireSource(eventId, sourceId)` records an observed source once. A composite
evidence record becomes complete only when every referenced source has been
acquired. `MissingSources(evidenceId)` retains the distinction between partial
and complete E03/E06. E03 never confirms identity; E04 does. The acquisition
layer records observations delivered by a caller; it does not decide which
conversation or scene is currently accessible.

Spoken sources record an attributed disclosure to `player`, including the
source and first acquisition event IDs. Documents remain observations rather
than inferred conversations. `ReplaySource` returns only an already acquired
source and changes no facts. Queries reject unknown IDs. The public fact
collections cannot be changed by callers.

Successful events retain their ID and canonical payload. An exact retry returns
`false` without repeating its effects; the same ID with a different payload
throws. Acquiring the same source or recording the same promise under a fresh
event ID also returns `false`. Rejected events do not consume their ID, so a
corrected retry can succeed. Recipient order does not change a send payload.
No event timestamps or external delivery mechanisms are introduced.

`ProtectPrivateContact` records a player promise, not Ren’s contact consent.
Promises, disclosures, and evidence never set an outcome. Case phase starts at
`NotStarted`, changes to `Investigating` on first acquisition, and changes to
`Resolved` only when a resolution is committed. Aftermath completion changes
phase once while retaining the selected outcome and disclosures.

The internal `CommitResolution` method validates known outcome/source/recipient
IDs and acquired sources before committing outcome and disclosures together.
It prevents a second outcome and copies caller-owned recipient arrays. It is a
storage seam for issue #8, not an eligibility or consent API. Only domain code
and the friend test assembly can call it. There is no public send operation yet.
Issue #8 must enforce the causal contract, current consent, selected material,
actual recipients, and confirmation before calling this seam. These tests do
not establish a legal narrative route through any ending.

Run the focused check:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseStateTests
```

Tests cover archive-first and conversation-first acquisition, partial evidence,
replay, event collisions, promises, attributed recipients, atomic resolution
storage, aftermath retries, unknown IDs, rejected batches, session isolation,
and defensive collections. Save/reload, scene UI, routing eligibility, and
whole-route rehearsal remain separate tasks. The existing native runner uses
disposable project/save state and retains private evidence as described in the
[development guide](../development.md).
