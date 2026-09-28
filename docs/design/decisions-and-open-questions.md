---
title: Decisions and Open Questions
type: documentation
status: draft
created: 2026-09-24
updated: 2026-09-28
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Decisions and Open Questions

[Project home](README.md) · Decision register dated 2026-09-24.

## Confirmed direction

| ID | Decision | Evidence from the user | Consequence |
|---|---|---|---|
| D-01 | Found family and belonging are central. | Answer to the family clarification. | Relationships and reciprocal care must be playable. |
| D-02 | Prioritize investigating people and uncovering unsettling truths. | Answer to the desired experience clarification. | Evidence and interpretation are core activities. |
| D-03 | The Common House is the leading candidate. | “The Common House is the top candidate.” | Develop a shared home and recurring residents. |
| D-04 | Combine the house with radio connections and investigations. | Proposed that the common house serve as a radio outpost. | Radio work and household life should affect each other. |
| D-05 | Begin the requisite design documents. | Current request. | Create a reviewable preproduction pack; implementation is not implied. |
| D-06 | Residents have uneven knowledge of anomalies. | Answer 1 to O-01. | Write distinct knowledge, experience, and beliefs for each resident. |
| D-07 | The player is a newcomer. | Answer 2 to O-02. | Introduce house routines and relationships through arrival. |
| D-08 | Alternate future after societal collapse. | Answer 3 to O-03. | Establish surviving local infrastructure; detailed history remains open. |
| D-09 | Favor suspense, mystery, and creepiness over fright. | Answer 4 to O-04. | Build tension through uncertainty and changes in familiar routines. |
| D-10 | Procedural means case-based stories. | Answer 5 to O-05. | Author cases; do not assume generated mysteries. |
| D-11 | Open source is a theme. | Answer 6 to O-06. | Explore shared knowledge and repair; no license requirement follows. |
| D-12 | Solo developer, desktop rig, Unity. | Answer 7 to O-07. | Plan a small Unity POC; hardware and installed version still need inspection. |
| D-13 | Present-day play stays at the outpost; other locations appear infrequently through investigable flashbacks or memories. | Answer 8 to O-08, including a tentative preference against leaving in reality. | Use this as the current spatial direction; do not plan routine expeditions. |
| D-14 | Protagonist is a blank slate for player self-insertion. | Answer 9 to O-09, expressed as the current preference. | Avoid a mandatory detailed biography; expression through present choices. |
| D-15 | Start with a POC before release structure. | Answer 10 to O-10. | Defer release, season, funding, and commercial production planning. |
| D-16 | Use Case 01 as the working POC backlog baseline. | “Use Case 01 as the working baseline” in the 2026-09-24 planning conversation. | Plan implementation around its six clues and three outcomes; names and story details remain provisional for owner review. |
| D-17 | Begin the base asset set with a style board, generated procedurally inside Unity. | 2026-09-25 answers: "Style board first" and "Procedural in Unity". | Editor code is the source for materials, textures, and study props; no third-party asset files. Revised by D-21. The board proposes directions for review; it does not approve a style or resolve Q-02. |
| D-18 | Store binaries in plain Git with a per-file size cap. | 2026-09-25 answer: "Plain Git, size cap". | `scripts/check_binary_sizes.py` fails on tracked binaries over 5 MiB. Git LFS is not configured; revisit if the asset set outgrows the cap. Revised by D-22. |
| D-19 | Writing follows a working asset set, mostly new cases, plus Case 01 revisions and world/character material. | 2026-09-25: "mostly new cases, but all of the above, obviously some will come as we make the asset". | Asset studies may prompt world and character notes; new cases still need owner review before becoming baseline scope. |
| D-20 | Target a high level of visual realism, rendered with HDRP. | 2026-09-25: "I want to go for a very high level of realism", then "HDRP (Recommended)". | The project uses HDRP 17.4.0 with Vulkan on Linux and linear colour. Lighting uses physical units and fixed exposure. Built-in shaders, including the style-board materials, must be ported before they render correctly. |
| D-21 | Source realistic materials from CC0 scans, combined with procedural work. | 2026-09-25, for the realism direction: "CC0 scans + procedural". Revises D-17. | Scanned files come only from Poly Haven or ambientCG under CC0 and live under `Assets/CommonHouse/Art/Sourced/`. Each file needs an entry in `docs/development/asset-provenance.json` with its source page, authors, retrieval date, and hash; `scripts/check_provenance.py` enforces this. Other sources or licenses need a new decision. |
| D-22 | Store textures and models with Git LFS. | 2026-09-25: "Git LFS". Revises D-18. | `.gitattributes` sends texture and model formats under `Assets/` through LFS. The 5 MiB cap still applies to plain-Git binaries such as documentation images. Gitea reported LFS enabled on 2026-09-26. |
| D-23 | Generate residents with MPFB2 (MakeHuman for Blender) from recipes kept in the repository. | 2026-09-28: at least a dozen characters, "realistic people", sourced or generated because the owner does not model or animate, and later ones "will appear to be the same"; then "we go with MPFB2". | Characters are rebuilt from JSON recipes with a pinned Blender, MPFB, and MakeHuman CC0 asset packs, so new residents match earlier ones. Exported characters and the packs are CC0; MakeHuman joins the CC0 sources allowed by D-21. Realism is below Character Creator or MetaHuman, and animation needs another source. |
| D-24 | Target late-1990s to early-2000s console 3D for models and graphics: the PS1 look with a PS2-era treatment. | 2026-09-28, after the style experiments: "I want to go with the PS1 aesthetic and I want to shoot for a GTA 3 style"; asked which parts of GTA 3, the owner chose only the PS2-era visual treatment; then "GTA 3 style just means the models and graphics. Not the gameplay or tone or anything. ... Think GTA 3, Silent Hill, Goldeneye: 007, Mission Impossible 64, Resident Evil, Mizurna Falls, System Shock". | Low-poly models with small textures, affine and unfiltered texturing, vertex lighting, fog, photographic faces on simple heads, and a soft, hazy 640x448 image. Gameplay, camera, tone, and story are unchanged by this entry; the first-person camera stays. Revises the visual direction of D-20 (high realism); HDRP remains the renderer, and CC0 sources and MPFB characters remain the asset pipeline. The playable outpost uses it; see docs/development/look.md. |

## Proposed working choices

| ID | Proposal | Reason | Revisit when |
|---|---|---|---|
| P-01 | Single-player, first-person exploration | Fits the expressed first-person interest and domestic intimacy | POC interaction review |
| P-02 | Newcomer role accepted as D-07; learning the night shift remains proposed | Introduces radio work through participation | Opening script review |
| P-03 | Uneven knowledge accepted as D-06; exact distribution remains proposed | Supports different resident accounts | Character and case review |
| P-04 | Authored cases accepted as D-10; conditional household scenes remain proposed | Keeps evidence coherent and responses contextual | POC authoring work |
| P-05 | No combat, death, or timed dialogue in the POC | Isolates investigation and supports D-09 | Playtest evidence |
| P-06 | Three rooms, two residents, one caller, one case | A bounded solo test | Case implementation |
| P-07 | Case 01 working baseline accepted as D-16; final story details remain proposed | Makes routing, consent, and inference concrete | Case review and playtest |
| P-08 | Add one short investigative memory only after the core loop works | Test the mechanic without building another large environment | Core-loop evaluation |

## Disposition of original questions

O-01 through O-06, O-08, and O-09 have current answers recorded in D-06 through D-14. O-03 is resolved at the broad setting level; detailed history is still open. O-07 is answered for team, engine, and available machine; budget, time availability, installed version, and hardware details remain unknown. O-10 is deliberately deferred by D-15. Do not ask these original preference questions again as though unanswered.

## Remaining POC questions

| ID | Question | What it changes | Needed before |
|---|---|---|---|
| Q-01 | How are memories accessed, whose perspective do they represent, and how reliable are they? | Evidence rules and presentation | Implementing a memory scene |
| Q-02 | What remains of infrastructure, how long ago was collapse, and where is the house? | Material culture and case context | Detailed world canon and representative art |
| Q-03 | Which Unity version/project location and desktop configuration are actually available? | Setup and a reproducible local build | Implementation |
| Q-04 | Does the selected Case 01 baseline provide the desired mystery and unease in play? | Revisions to script and clue structure; baseline selected by D-16 | Finalizing case content after review and playtest |
| Q-05 | How should the player express identity through name, dialogue, and voice? | Self-insertion and asset needs | POC dialogue presentation |

These questions can be resolved at the point of need. They do not justify expanding into release planning.

## Deferred choices

Release structure, commercial platforms, full-game length, funding, price, publication, release date, professional voice production, and final title are deferred until after the POC. Unity is selected; version and tooling are not yet verified. Multiplayer, open-world structure, generated cases, and extensive equipment simulation are outside the first test.

## Change log

2026-09-24, v0.1: Created the initial pack from the design conversation. Separated four creative commitments from agent proposals. Added a provisional case and an unexecuted evaluation plan.

2026-09-24, v0.2: Recorded the user's ten answers as D-06 through D-15. Superseded the original unanswered questions and engine-selection assumptions. Updated all design documents for uneven knowledge, newcomer self-insertion, alternate-future collapse, restrained suspense, authored cases, thematic open source, solo Unity development, location-bound present-day play, investigable memories, and POC-first scope. The case's cast, events, and numerical scope remain proposals.

2026-09-24, planning update: Recorded D-16 and the owner-selected Case 01 working baseline. This does not approve final names, story details, or a release scope.

2026-09-25, asset direction: Recorded D-17 through D-19 for procedural style-board work, plain-Git binary storage with a 5 MiB cap, and the owner's writing order. No visual style, license, or new case is approved by these entries.

2026-09-25, rendering: Recorded D-20. The owner asked for a very high level of realism and chose HDRP. Asset sourcing and binary storage were also revisited that day; see D-21 and D-22. No visual style is approved by this entry.

2026-09-25, sourcing and storage: Recorded D-21 and D-22. CC0 scans from two named sources join procedural generation, with per-file provenance. Textures and models move to Git LFS. D-17 and D-18 remain as the earlier choices they revise. No scanned asset is added by these entries.

2026-09-28, characters: Recorded D-23 after comparing Character Creator 5, MetaHuman, MPFB2, and Microsoft Rocketbox with the owner. No resident design is approved by this entry.

2026-09-28, look: Recorded D-24 after experiments with N64, PS1, pre-rendered, isometric, and anime looks, which stay on their branches for reference. It covers models and graphics only.

When a choice changes, record its date, rationale, affected documents, and remaining limits. Preserve superseded decisions explicitly.
