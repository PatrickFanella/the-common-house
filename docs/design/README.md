---
title: The Common House
type: work
work-type: software
status: active
stage: drafting
created: 2026-09-24
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# The Common House

A game concept about found family and unsettling investigations, centered on a shared home that operates a radio outpost.

Working title. Creative design pack updated 2026-09-29. These documents distinguish confirmed direction from proposals; implementation and verification status belong in the [repository README](../../README.md) and development reports.

## Start here

Read the Concept Brief for the pitch, then the Game Design Document for the proposed player experience. Read Decisions and Open Questions before treating any proposal as settled.

| Document | Purpose |
|---|---|
| [Concept Brief](concept-brief.md) | Premise, audience hypothesis, design pillars, and creative boundaries |
| [Game Design Document](game-design-document.md) | Core loop, progression, interactions, failure, and scope |
| [Narrative and World Bible](narrative-and-world-bible.md) | World assumptions, character functions, themes, and story structure |
| [Incident and Network — Author Truth](incident-and-network-author-truth.md) | Private causal history, AI limits, disrupted geography, and open world rules; contains spoilers |
| [Episodic Story and Case Writing](episodic-story-and-case-writing.md) | Emotional release, social investigation, source-bound signals, domestic traces, and a proposed case seed |
| [Episode authoring system](episode-authoring-system.md) | Reusable workflow, script/contract/handoff templates, and review gates |
| [Motion detected episode package](episodes/motion-detected/README.md) | First drafted future episode: playable script, evidence contract, and implementation handoff |
| [Radio and Investigation Systems](radio-and-investigation-systems.md) | Evidence, transmissions, conversations, routing, and consequences |
| [Art Audio and UX Direction](art-audio-and-ux-direction.md) | Visual and sound direction, interface, accessibility, and asset priorities |
| [Style Board v0.1](style-board.md) | Procedurally generated room studies, materials, and palette for review |
| [Technical Design Outline](technical-design-outline.md) | Solo Unity POC, state model, tooling, saves, and technical questions |
| [Proof of Concept Plan](proof-of-concept-plan.md) | Proof-of-concept scope, evaluation, dependencies, and risks |
| [Case 01 - The Road Is Clear](case-01-the-road-is-clear.md) | A complete provisional case for testing the concept |
| [Research and References](research-and-references.md) | Influence map, sources, and limits of the research |
| [Decisions and Open Questions](decisions-and-open-questions.md) | Confirmed direction, provisional choices, and unresolved decisions |

## Document contract

- **Confirmed:** explicitly selected by the user in the design conversation.
- **Proposed:** a concrete draft choice for discussion or testing; not approved canon.
- **Open:** unresolved and potentially material to scope or implementation.
- **Deferred:** intentionally outside the first prototype; reconsider if evidence supports it.

All design details are proposed unless identified as confirmed in Decisions and Open Questions. Drafts describe intended behavior, not existing features. The sample case demonstrates one possible story and does not establish the final plot.

## Ownership and change control

This repository's `docs/design/` directory is the authoritative design source. The user owns creative decisions; agent-authored details remain proposals until accepted. Update the owning document first, then reconcile summaries and case examples. Record material accepted choices in Decisions and Open Questions with their date and reason.

The original Ars Malefica notes are retained as frozen migration snapshots. Continue design edits here. Source file hashes and the import boundary are recorded in [Import provenance](../import-provenance.json).

## Current review sequence

1. Use the decision register to distinguish accepted writing choices from proposals.
2. Read the author truth before drafting anomalies or global-arc clues; resolve an open rule before making a case depend on it.
3. Draft human stories using the case-writing guide, including an original signal source and a domestic trace.
4. Review future cases and any Case 01 revisions with the owner before changing the working baseline. Memory experiments and release structure remain outside this writing update.

Confirmed direction now includes uneven resident knowledge, a newcomer designed for player self-insertion, authored case-based stories, and open source as a theme. Present-day play stays at the outpost; infrequent other locations appear through investigable flashbacks or memories. See the decision register for the full record.

## Relationships

- **Up:** [Repository README](../../README.md)
- **Source:** User/assistant design conversations beginning 2026-09-24, including the 2026-09-29 writing room; external references in Research and References.
- **Output:** A reviewable preproduction design pack and a proposed Unity proof of concept.
