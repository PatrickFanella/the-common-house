---
title: Radio and Investigation Systems
type: documentation
status: draft
created: 2026-09-24
updated: 2026-09-24
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Radio and Investigation Systems

[Project home](README.md) · Proposed system contract. Case 01 supplies a concrete example.

## Radio model

The first prototype uses a small fictional set of channels and routable endpoints. Tune to a channel, inspect the signal indicator, listen or replay, record an excerpt, and choose a destination. Physical patching is a candidate presentation for routing; a labeled menu is an equivalent accessible control.

Avoid requiring real radio expertise. The outpost exists in a confirmed alternate future after societal collapse. Its precise technology, surviving authorities, and local operating customs remain undecided. These are fictional interaction rules, not claims about real radio capabilities.

## Transmission lifecycle

Available → received → reviewable → eligible for a response → resolved or deliberately left open.

A transmission record contains a stable ID, source endpoint or unknown source, arrival phase, audio/transcript references, observable metadata, availability conditions, response options, and effects. The caller's asserted identity is separate from a verified identity.

Essential calls remain in the log or can be repeated. Advancing a household scene cannot silently delete the only path to a necessary clue. Replaying a recording must not repeat its narrative effects.

## Evidence and interpretation

| Information kind | Example | Notebook behavior |
|---|---|---|
| Observation | Archive tape contains the kitchen bell at a marked time | Link to the recording and timestamp |
| Attributed claim | Ivo says the caller has never visited | Preserve speaker, context, and wording |
| Hypothesis | The caller once lived at the house | Mark as provisional; allow revision |
| Corroborated fact | A dated roster and testimony identify the caller | Show supporting sources without asserting unrelated motives |

The notebook collects inspectable evidence and supports side-by-side comparison. It should not announce hidden truths. For the prototype, a small authored set of hypothesis prompts is sufficient; freeform notes are optional and must never require automated semantic judgment.

Distinguish discovering a contradiction from proving a lie. A witness can be mistaken. Case writing owns the ground truth; the interface shows what the player has grounds to know.

## Conversations and sharing

Questions unlock from observations, previous dialogue, or direct requests for help. Evidence can be shown to a resident through an explicit share action. Remember who received what and whether the player promised confidentiality. Consulting someone may create both a new lead and a social consequence.

Characters may decline to answer. If their answer is essential, provide a second evidence route or a later opportunity. Do not let a relationship branch remove every path to resolving the case.

## Routing contract

Before a consequential transmission, display source, recipient, selected message or excerpt, and whether other household listeners are included. Give the player a chance to cancel before sending. The prototype does not model hidden technical eavesdropping; if introduced later, establish that capability before its consequences matter.

Potential actions include a private connection with consent, a message relay, a request for verification, or an explicit pause pending investigation. Once sent, information cannot be fictionally unsent. Later dialogue can acknowledge correction or apology. Loading a prior save remains available.

## Memory evidence

Investigable memories and flashbacks may provide attributed evidence. Store their source, perspective, and the observed detail separately from present-day corroboration. Replaying a memory must not strengthen a claim automatically or repeat its effects. The POC can omit this subsystem initially; its access rules remain open.

## Consequence state

Track concrete events: identity confirmed, private contact offered, a resident's boundary respected or breached, a promise made, a message shared, and a case resolution selected. Avoid a single morality or trust total. A person can appreciate your help and resent a disclosure.

Keep immediate feedback distinct from delayed effects. The first prototype supplies both a radio response and a domestic aftermath. Larger inter-case effects are deferred.

## Repair interaction

A repair should reveal a cause or dependency. In the sample case, comparing a routing label with the physical switch explains how to separate a private headset from the room speaker. This requires one understandable intervention. Do not add consumable components, crafting recipes, or electronics puzzles to this test.

## Fairness and recovery

Essential inferences need multiple supports or an alternate access path. Name uncertainty explicitly. Never randomize the actual culprit, speaker, or timeline after clues have been authored. Repeated dialogue and recordings are safe to revisit. Empty archive searches suggest a known lead instead of inventing facts.

Optional hints escalate from identifying an available lead, to pointing out a relevant comparison, to explaining that comparison. A player can complete all essential reasoning from transcripts and visible metadata without hearing audio.
