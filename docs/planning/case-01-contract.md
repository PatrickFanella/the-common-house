# Case 01 causal and branch contract

Working implementation contract for **The Road Is Clear**, based on the
owner-selected baseline D-16. Names, dialogue, history, and supernatural details
remain provisional. This document specifies observable state and recovery
behavior for the POC; it does not establish final canon or implemented code.

Owning sources: [case truth](../design/case-01-the-road-is-clear.md),
[confirmed decisions](../design/decisions-and-open-questions.md),
[radio and investigation](../design/radio-and-investigation-systems.md), and
[POC evaluation](../design/proof-of-concept-plan.md).

## Factual spine and information boundaries

Mara is Ren's former network name. Ada is the known past operator and her
present call is genuine. Ivo previously disclosed Ren's location without
permission and removed the log page documenting it. He does not control the
present transmission. The invitation phrase concerns contact, not safe travel
or universal trustworthiness. These are author facts; the notebook must not
present them until supported by what the player has actually learned.

The player first receives ordinary care from Ren, then Ada's request creates
a reason to investigate. Ren's wish to control contact remains meaningful
regardless of how much corroboration the player acquires. No amount of
evidence automatically becomes permission.

## Evidence map

| Record | Source and observable content | Permitted inference | Access and recovery |
|---|---|---|---|
| E01 | Current Ada call and full transcript; Ada names herself, requests Mara, and gives the invitation phrase | A caller claims to be Ada and wants contact with Mara; neither identity is yet established for the player | Automatically logged on receipt; replay/reopen indefinitely, no countdown |
| E02 | Dated archive tape identifies Ada and contains the captioned kitchen bell | Supports Ada's past presence in this house; not proof of every current claim | Archive shelf label or Ivo's archive suggestion; remains available after leaving and returning |
| E03 | Old roster names Mara at the workshop; Ren's current repair note carries the matching distinctive mark | A possible Ren/Mara connection, explicitly provisional | Legible roster and note views; side-by-side comparison; retain both sources; never required for E04 |
| E04 | Private conversation in which Ren confirms the former name and describes the unwanted relay | Confirms Ren/Mara identity and attributes the prior breach account to Ren | E01 permits the private question directly; E03 is not a gate; cancellation permits returning later |
| E05 | Removed log page records the unauthorized relay and Ivo's involvement | Independent documentary support for the prior breach | Discuss the visible log gap with Ivo; he supplies the page or points to it; no mandatory trespass or unique timed encounter |
| E06 | Ada's mundane shared-repair account plus Ren's corroborating account | Supports the present caller being the Ada Ren knows; does not prove all motives or grant consent | Both follow-up conversations remain available before resolution; retain each half separately until both exist |

Evidence records can contain more than one source observation. In particular,
E03 includes the roster/note pair and E06 includes two attributed accounts.
A partial record must not look complete. The canonical six IDs remain stable;
implementation may assign subrecord IDs without adding a seventh required clue.

There is no required audio-recognition step. E02's bell has an observable sound
caption, E03's handwriting has a readable textual representation, and all
spoken content has speaker-attributed text. Captions describe the observation;
they do not reveal its significance for the player.

## Minimal state vocabulary

Keep immutable case content separate from mutable player state. The model
needs only acquired observations, attributed accounts, recipient disclosures,
route inspection, current contact authorization, draft, resolution, and
aftermath completion. Stable event IDs permit safe retry without double effects.

- `identity_confirmed`: set only by E04, never by opening or comparing E03.
- `repair_accounts`: separate Ada and Ren observations; both complete E06.
- `private_route_verified`: set by inspecting the diagram and actual connection.
- `contact_authorization`: unknown, refusal-authorized, or private-contact-authorized.
- `resolution`: none, R1, R2, or R3. Defer is not a resolution.
- `disclosures`: what was transmitted, to which actual recipients, in which event.
- `aftermath_completed`: records the selected outcome's applied aftermath once.

The enum names are a suggested implementation vocabulary, not a requirement
to build a general rules engine. Preserve the behavioral distinctions if the
implementation uses different names.

## Authorization and transition rules

| Action | Preconditions | Effect and non-effects |
|---|---|---|
| Receive call | Opening reaches Ada's call | Record E01 once and keep it reviewable; do not verify her claim automatically |
| Inspect/replay/compare | Relevant material available | Read or acquire the observed material once; replay has no narrative effect |
| Ask identity privately | E01 known, unresolved case | Obtain E04; Ren initially declines immediate contact and authorizes refusal |
| Corroborate repair | Relevant resident/caller question available | Record each attributed half; both complete E06; neither changes authorization |
| Inspect actual route | Inspectable diagram/connection available | Identify actual headset/speaker listeners; set route verification, not consent |
| Offer private assurances | E04 and E06 complete, private route verified, unresolved case | A separate authored conversation can record Ren's explicit private-contact agreement |
| Prepare transmission | Case unresolved | Record a draft; before E04, identify the unresolved recipient and prohibit final send |
| Cancel confirmation | A draft exists | No transmission or resolution; keep investigation available |
| Defer | Case unresolved | Leave case open, preserve draft and essential leads; no fourth ending |
| Confirm valid send | Corresponding outcome predicate holds | Apply one resolution and its actual recipient disclosures atomically |
| Revisit aftermath | A resolution exists | Show the matching authored scene; apply its consequential effects only once |

A repeated E04 conversation must not reset later explicit agreement to the
initial refusal. A repeated agreement must not create another consent event
or send anything. Authorization is scoped to the selected contact method:
private agreement never authorizes broadcasting to the room.

The POC has no separate consent-reversal branch. A new choice to reverse a
previous agreement needs an authored rule before implementation; do not invent
one to satisfy a UI state. Loading an earlier save intentionally restores the
authorization state of that save.

## Resolution predicates and aftermaths

All outcomes require an unresolved case, E04, a displayed message/recipient
summary, and explicit confirmation. The summary identifies the source, actual
recipient, selected material, and any included household listeners.

| Outcome | Additional predicate and action | Immediate result | Required aftermath |
|---|---|---|---|
| R1: Private contact | E06 complete; private route verified; explicit private-contact authorization; select verified private headset route | Ren and Ada speak privately; only information Ren later shares becomes player knowledge | Ren invites the player into a routine repair and acknowledges the respected boundary |
| R2: Refusal relayed | Current refusal authorization; relay the refusal to Ada | Ada accepts it and leaves a way to reconnect; the message's undisclosed contents stay unknown | Ren remains available and appreciative; case is complete |
| R3: Public disclosure | Select actual room speaker; show the explicit warning that public disclosure is not authorized | Ren ends the exchange; the disclosure record includes actual household listeners | Ren asks for distance; a later acknowledgment and repair attempt remain possible without erasing the breach |

R2 is not an incomplete or inferior outcome. R3 does not depend on a hidden
switch trick: identify the actual room listeners before confirming, even if
the player has not completed the private-route inspection. If the UI cannot
identify its actual recipients, it must not send.

After explicit private agreement, an old refusal draft is stale and cannot be
sent as if it reflected Ren's current wishes. Validate the current predicate
again at send time, not only when a button was first enabled.

## Reference routes

These are deterministic test traces, not a forced walkthrough. Opening and
E01 precede investigation. Route-inspection and optional clue order may vary.

| Trace | Ordered actions | Expected final state |
|---|---|---|
| Archive-first R1 | E01 → E02 → E03 → private E04 → optional E05 → both E06 accounts → inspect private route → offer assurances → explicit agreement → confirm private contact → aftermath | R1 once; private recipients only; repair invitation available |
| Conversation-first R1 | E01 → private E04 → route inspection → both E06 accounts → explicit agreement → optional E02/E03/E05 → confirm private contact → aftermath | Same R1 facts; optional clues add context, not a second reveal |
| Minimal refusal R2 | E01 → private E04 → confirm authorized refusal → aftermath | R2 complete; no E02/E03/E05/E06 requirement; Ada's message remains private |
| Informed disclosure R3 | E01 → private E04 → select room speaker → read recipient/authorization warning → confirm → aftermath | R3 once; household disclosure retained; no forced forgiveness |
| Defer/recover | E01 → private E04 → draft refusal → cancel → defer → revisit any remaining lead → resume refusal | No resolution until final confirmation; R2 remains reachable while refusal is current |
| Partial corroboration | E01 → private E04 → only one E06 account → inspect route → attempt private contact | R1 unavailable; no inferred consent; second account and refusal route remain reachable |

For each outcome, also exercise archive-first and conversation-first prefixes,
repeated calls/questions, optional-content omission, and transcript-only input.
E06's two accounts must remain recoverable whichever resident/caller is
consulted first; a missing half is a lead, not permission to fabricate it.

## Repeat, save, and recovery invariants

1. Evidence acquisition is set-like. Replay, comparison, and notebook opening
   cannot duplicate evidence, strengthen a claim, or change authorization.
2. A declined optional question cannot remove every completion route. E01,
   private E04, and authorized refusal remain recoverable while unresolved.
3. No scene transition, pause, hint, or elapsed time silently deletes a lead.
4. Validate the recipient and authorization at confirmation. Double submit
   commits one resolution and one set of disclosures.
5. Save/reload before sending restores draft and facts without transmitting.
   Reload after sending restores outcome and disclosures without retransmission.
6. Restoring an aftermath-completed save cannot apply its effects again.
7. Incompatible or corrupt saves produce a visible error and preserve the file;
   never hide a failure by silently resetting the case.
8. Text-only and audio-muted paths expose the same essential facts and choices.

## Remaining creative decisions and excluded work

Final dialogue, names, timing, detailed world history, and how the mystery feels
remain owner-review and playtest questions. No mandatory protagonist identity
is introduced. Scene timing must give household care room to register without
adding a timed response. The player is never required to leave the outpost.

The optional memory and final anomalous receiver beat are deferred from this
first implementation backlog. Neither may supply a required clue or overturn
the caller's identity. Human observation, not a causal graph or unit test, must
establish whether the identity inference is understandable and emotionally
meaningful. See the proposed evaluation thresholds in the POC plan.

## Verification boundary

`python3 scripts/check_docs.py docs/planning/case-01-contract.md` checks this
file's local links and nonempty content. It does not prove the causal model.
This pass was checked manually against the owning case document. Future
CaseContentTests, CaseStateTests, RoutingRulesTests, and CaseRehearsalTests
implement executable assertions for the rules above in issues #6–#9.
