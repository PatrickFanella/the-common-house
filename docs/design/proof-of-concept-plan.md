---
title: Proof of Concept Plan
type: documentation
status: draft
created: 2026-09-24
updated: 2026-09-24
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Proof of Concept Plan

[Project home](README.md) · Solo Unity proof-of-concept plan. The user is the sole developer and will use their desktop rig. Scope and evaluation details below remain proposals; no schedule or budget is committed.

## Scope of this plan

The user has chosen to start with a POC before considering release structure. No release schedule, season plan, commercial production phase, or vertical-slice commitment is part of this revision.

## First question

Can investigating a radio contact change how the player understands and treats a resident they have begun to care about?

The first playable should answer this before we expand the cast, mythology, or map. Its detailed case is Case 01 - The Road Is Clear.

## Bounded scope

Target 20–30 minutes, to be measured rather than assumed. Include three spaces (radio room, kitchen, archive/workshop), two residents, one caller, six core evidence records, one privacy-routing interaction, three case resolutions, and a domestic aftermath for each resolution.

Use temporary visuals and dialogue. The prototype needs complete text and caption support, but does not need final voice acting. Present-day play remains at the outpost. No external expedition, combat, day/night simulation, generated cases, or full relationship simulation is required. Suspense, mystery, and creepiness guide the tone.

An optional extension adds one short investigable memory after the core loop works. Reuse a room in an earlier state if possible. Define the memory's source and reliability before implementing it. The baseline case remains playable without the extension.

## Milestones and exit evidence

| Milestone | Deliverable | Exit evidence |
|---|---|---|
| M0: Case review | Factual timeline, clue map, dialogue outline, branch outcomes | No unresolved causal contradiction; every required inference has support |
| M1: Narrative test | Paper, linked-text, or equivalent playable case | A reader can investigate, choose, and reach an aftermath without author intervention |
| M2: Unity blockout | Three rooms, inspectable evidence, basic radio and routing on the user's desktop | Complete route playable; revisiting evidence and alternate discovery order work |
| M3: Instrumented playtest build | Save/load, captions, optional hints, branch coverage | All three outcomes reachable; no essential audio-only clue or case softlock |
| M4: Concept evaluation | Observation notes and a revised design | Evidence supports continuation, narrowing, or a specified redesign |
| Optional memory experiment | One bounded investigable flashback/memory | Players distinguish remembered information from corroborated fact; return to the outpost preserves state |

Stop at a POC evaluation and a concrete next design decision. The rough playable does not prove final art quality, broad hardware compatibility, commercial feasibility, or release readiness.

## Initial playtest protocol

Proposed first round: five participants familiar with narrative games, including at least one transcript-only session. This is qualitative design feedback, not statistical market validation or a complete accessibility audit. Obtain consent for observation or recording; keep notes anonymous where possible.

Give a short control introduction without explaining the case. Observe where players look, which comparisons they make, where they request help, and whether they understand the recipient before sending. Afterward, ask them to reconstruct events and explain their choice. Avoid leading questions about whether they liked the residents.

## Proposed decision thresholds

- At least four of five can explain the central identity inference using two supporting observations without being told the answer.
- At least four of five correctly identify who will hear their chosen transmission before they send it.
- At least three of five recall a concrete act of household care and explain why a resident's wishes affected their decision.
- Every participant has a reachable completion path; required hints and interventions are recorded rather than concealed.
- The transcript-only route preserves all essential evidence and decisions.

These are initial revision triggers. If a threshold is missed, inspect the cause and change the relevant design before repeating the test. Do not generalize a five-person result into claims about the eventual audience.

## Production risks

| Risk | Early signal | Response |
|---|---|---|
| Players listen passively | No new question follows a call | Add a useful comparison or consequential response |
| Family exists only in dialogue claims | Players remember secrets but no care | Strengthen reciprocal everyday scenes |
| Ambiguity feels arbitrary | Players cannot cite evidence | Repair the factual spine and clue redundancy |
| Branching multiplies content | Each disclosure demands unrelated scenes | Reuse locations and case phases with state-specific responses |
| Radio is inaccessible | Clues depend on hearing or precise tuning | Add equivalent captions, metadata, and control alternatives |
| Unique vignettes expand scope | Every case requires a new subsystem | Test new meanings through the shared interaction vocabulary |
| Voice/art costs dominate early | Revisions require expensive asset replacement | Keep placeholders until the case structure stabilizes |

## Solo development constraints

Use placeholder art, simple character presentation, local authored dialogue, and a compact state model. Keep a single case playable as features are added. Avoid building a general quest framework, large asset library, or full house before the investigative loop works.

Inspect the actual Unity installation and target desktop before committing technical settings. Project location, Unity version, machine specifications, available time, and spending limits are not yet verified. Measure iteration effort during the POC to inform later scope decisions; do not extrapolate a full-game schedule now.

## POC evaluation additions

Alongside the inference and routing checks, ask players what created suspense and whether any moment felt confusing rather than mysterious. Record whether the newcomer felt open to self-insertion or carried an unwanted prescribed identity. If the memory extension is included, ask players which details they consider corroborated and why.

The proposed five-person round is an eventual external check, not a staffing requirement or a condition for beginning implementation. Start with solo route verification; author testing cannot establish whether the mystery is understandable to a new player.

## Review record

The local prototype and automated scene/player checks are complete. The [initial owner evaluation](../playtests/poc-evaluation.md) records one exploratory refusal run and Save use; Load and understanding were not established. The proposed new-player round has not been reported. Record build/version, date, participant count, conditions, observations, assistance given, failures, and resulting changes as testing continues. Keep failed and incomplete results alongside later improvements.
