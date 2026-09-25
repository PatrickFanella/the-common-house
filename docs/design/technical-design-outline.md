---
title: Technical Design Outline
type: documentation
status: draft
created: 2026-09-24
updated: 2026-09-24
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Technical Design Outline

[Project home](README.md) · Unity is selected for a solo developer working on their desktop rig. Architecture below remains proposed. The Unity version, installed packages, project repository, and build target have not been inspected.

## Technical objective

Support an authored investigation in a small explorable home. Keep narrative facts reproducible, content easy to revise, and essential interactions accessible. The first technical proof is one coherent case that survives save/load and different evidence orders.

## Candidate boundaries

| Component | Responsibility |
|---|---|
| Interaction layer | Movement, object focus, inspection, radio controls, accessible alternatives |
| Narrative state | Stable facts, case phase, promises, disclosures, decisions, and prerequisites |
| Content runner | Select authored scenes and dialogue from explicit conditions |
| Radio service | Available calls, playback, recording references, endpoints, and routing |
| Evidence notebook | Sources, excerpts, claims, comparisons, and hypotheses |
| Persistence | Save version, narrative state, collected evidence, settings, and current scene |
| Authoring validation | Missing IDs, invalid references, unreachable essential content, and effect duplication |

These are logical responsibilities, not a requirement for separate processes or services. A local single-player application is the baseline proposal.

## Content records

Use stable identifiers for characters, endpoints, transmissions, scenes, evidence, and decisions. A scene includes prerequisites, participating characters, content references, available actions, and explicit effects. A case includes its factual timeline, essential inferences, evidence routes, branch prerequisites, and outcomes.

Keep author-only truth distinct from player-visible evidence. Dialogue should reference state; it should not be the sole storage for state changes. Record a disclosure's recipient and evidence ID so later scenes can respond accurately.

## State transitions and saves

Apply each consequential action once. Replaying audio and reopening a document are read operations. Persist evidence acquisition and disclosures independently of presentation animations. Save before final routing; after routing, persist the resulting decision and aftermath eligibility together.

Version the save format. If a development content change invalidates a save, detect it and explain the incompatibility. Do not silently reset a case. Production migration requirements depend on the eventual release strategy.

## Narrative authoring approach

Start with authored conditions and a small number of case phases. A storylet-style scene record is a candidate: prerequisites select available content, and effects change later availability. Choose the authoring workflow within Unity before adding an external dialogue tool. Start with the small case's actual needs rather than building a general narrative framework.

No generative dialogue, speech recognition, cloud dependency, or live radio access is required. All calls can be fictional local content. The user means authored case-based stories by procedural. Procedural case generation is outside the chosen approach.

## Audio and accessibility data

Keep each audio segment associated with transcript text, speaker attribution when known, and captioned environmental cues. Support clean replay and navigation by excerpt. Do not place essential clues solely in signal-processing effects. UI input actions should support both physical controls and a simpler labeled interaction.

## Debug and verification needs

A development view should show current case phase, known evidence IDs, disclosures, and available scenes without appearing in the release interface. Test the complete intended route, out-of-order discovery, skipped optional conversations, repeated playback, and reload before/after routing. Validate that every outcome has a reachable aftermath.

Content validation can check references and prerequisites; it cannot prove that players understand the mystery. Human playtests remain necessary.

## Solo Unity POC approach

Use a small local Unity project with placeholder rooms and characters. Keep case data, runtime state, and presentation separate enough to revise dialogue without rebuilding scene logic. Proposed implementation uses simple C# state records and locally authored content; exact asset format and UI tooling await inspection of the actual project. No package or Unity API choice is validated by this document.

The first scene contains the three-room outpost. If an investigative memory is added, a controlled variant of an existing room can test the idea. Isolate the memory scene's state from present-day changes and explicitly carry discovered evidence back on return. Verify that entering, replaying, exiting, and saving a memory do not duplicate effects or alter present-day props accidentally.

Before implementation, verify Unity version, installed packages, operating system, GPU, available memory, project location, and a working empty build on the user's desktop. The user has confirmed the rig and Unity, but its specifications have not been measured here. Budget and weekly availability remain unknown; they do not require release planning to begin a bounded POC.

Paper review can catch causal errors before Unity scene work. It supports the Unity POC rather than replacing the engine decision. Open source is an in-world theme, with no confirmed release-license requirement.
