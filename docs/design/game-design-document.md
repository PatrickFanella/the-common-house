---
title: Game Design Document
type: documentation
status: draft
created: 2026-09-24
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Game Design Document

[Project home](README.md) · Proposed design v0.2 within the confirmed creative direction. Detailed systems belong in Radio and Investigation Systems.

## Player role and viewpoint

Confirmed role: a newcomer, conceived as a blank slate for player self-insertion. Learning the night shift is the proposed introduction. Let the player discover routines and relationships through participation.

First-person movement provides physical presence in the house. Close inspection supports objects, station logs, maps, equipment, and recordings. Dialogue choices express questions and commitments. Avoid a mandatory detailed biography, predetermined ideology, or surprise personal connection that rewrites the player's identity. Let choices express their attitudes. Naming, customization, and protagonist voice remain open; a text-choice protagonist is the economical POC proposal.

## Core loop

1. Spend time in the house and notice an ordinary need or change in routine.
2. Receive a call or discover a missed check-in.
3. Record relevant observations and investigate their context.
4. Consult residents, compare accounts, and follow available leads.
5. Choose how to relay information or arrange contact.
6. Experience the immediate response and a later household consequence.

Allow returns to earlier steps. Players should be able to revisit evidence, revise their interpretation, and postpone a consequential action while they pursue another lead.

## Interaction vocabulary

| Action | Player input | Intended purpose |
|---|---|---|
| Inspect | Look closely, manipulate a simple object, read its context | Notice evidence embedded in an inhabited space |
| Listen | Tune, replay, scrub a recording, read its transcript | Identify words, timing, voices, and contradictions |
| Compare | Place relevant records or excerpts beside each other | Support an inference without relying on memory alone |
| Ask | Choose a question anchored in something observed | Obtain another account or challenge an explanation |
| Share | Select evidence and a recipient | Seek help while revealing the investigation's scope |
| Connect | Route or relay a message after confirming its destination | Act on the investigation and affect a relationship |
| Repair | Diagnose one meaningful equipment fault | Restore access and reveal a history or dependency |

## Session and case structure

Use a flexible shift structure: domestic scene, radio work, investigation, decision, aftermath. Advance major phases through authored events or an explicit end-shift action in the first prototype. Avoid a continuously running clock until it proves useful.

Each case resolves a local human question while leaving consequences in the house. Standalone episodic stories connect to a global arc, as selected in the 2026-09-29 writing conversation. A later case can revisit an earlier promise, disclosure, or domestic trace. Not every case needs a revelation about the incident or AI.

Stories center on someone dealing with something and releasing emotion. Investigation is social: consulting a resident or sharing a source changes who knows what and can affect a relationship. Emotional aftermath should become visible in routines, objects, boundaries, and responsibilities. The [case-writing guide](episodic-story-and-case-writing.md) provides the authoring framework.

The selected Season 1 writing structure is an intro, Episodes 1–8, and a separate season finale that leaves the game story open. The [roadmap](season-01-story-roadmap.md) tracks writing order; existing cases have no automatic numbered slot. Release/distribution structure, later-season count, and final game endings remain deferred. This creative direction adds no cases or mechanics to the current POC backlog.

## Place, memory, and suspense

Present-day play stays at the radio outpost in an alternate future after societal collapse. The house occupies a converted telecommunications office. The wider world reaches it through arrivals, transmissions, objects, and records. Disrupted terrain isolates safe pockets, while following existing powerlines or underground networking is a later, partially reliable travel discovery. This world rule does not add player expeditions.

Transmitted glimpses of other lives require an actual source such as a call, chat, camera, or monitor. Preserve its coverage and uncertainty; see [signal rules](radio-and-investigation-systems.md) and the [author truth](incident-and-network-author-truth.md). The owner expects a good number of flashbacks (D-40), which may allow investigation of other places; routine external expeditions are outside the direction.

A proposed memory interaction lets the player revisit a limited scene, inspect a detail, then compare it with present-day evidence. Identify whose account the scene represents and its source. A remembered detail is not automatically an independently verified fact. Memory access, perspective, reliability, and whether the experience is supernatural remain open.

Build suspense through delayed answers, changed routines, contradictory accounts, and familiar sounds in unexpected contexts. Avoid making chase sequences, jump scares, or repeated death the core loop.

## Progression

Progress through knowledge, access, responsibilities, and relationships. Recognizing a call sign can open a conversation; understanding a repair can restore a station function. Equipment upgrades are deferred. Information access should have alternate routes when a single missed interaction would otherwise block the case.

Use specific relationship facts such as a promise kept or a private recording shared. A public friendship score is not proposed. Residents can disagree with the player and still care for them.

## Failure and consequences

The first case has no death or combat failure. Mistaken conclusions lead to conversations and consequences rather than a full restart. Technical routing errors should be clearly recoverable before transmission. Before a consequential send, show the recipient and material being shared. Do not require confirmation for routine tuning or playback.

Incomplete evidence may leave an outcome uncertain. Distinguish that uncertainty from a softlock. Essential clues need an alternate source or later opportunity. Save before major decisions and support deliberate replay; no punitive save restrictions are proposed.

## Proposed first playable boundary

A solo Unity POC with a proposed 20–30 minute target, subject to playtest: three accessible rooms, two resident NPCs, one remote caller, one case, one routing decision with three possible resolutions, and a household aftermath. A small equipment interaction supports the case but does not become a separate crafting system. A short memory is an optional POC extension, not part of the baseline three-room case; decide its inclusion after the core loop works.

Deferred: multiplayer, open world, procedural case generation, combat, complex NPC schedules, extensive repair simulation, full voice production, live radio or internet integration, and a final supernatural taxonomy.

## Player experience requirements

The case must be completable without identifying a voice by ear, discerning pitch, reading small handwriting, or acting on a timer. Essential sound clues have equivalent accessible evidence. The notebook separates direct observations, attributed statements, and player hypotheses. Assistance can suggest an available lead without supplying the conclusion.

## Acceptance mapping

The prototype plan owns evaluation thresholds. Its questions should test each confirmed foundation: whether players care about a resident, make an evidence-supported inference, understand radio routing, and perceive the house as both home and outpost. Passing these checks would support further development; it would not prove the full game commercially viable.
