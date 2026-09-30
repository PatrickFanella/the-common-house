---
title: "Read receipt: agent handoff"
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Read receipt: agent handoff

[Brief](../design/episodes/read-receipt/README.md) · [Script](../design/episodes/read-receipt/script.md) · [Contract](../design/episodes/read-receipt/case-contract.md).

## Assignment and actual-code boundary

Implement one native issue at a time after inspecting its body, blockers, and current source. This document does not queue work. The 2026-09-29 writing pass inspected Case 01's separate content/domain/interaction/persistence structure and the isolated intro controller. The intro content and state load independently, but its playable controller covers only threshold, reception, and welcome. No generic season coordinator or runtime for namespace RR is implemented.

Do not put this case into Case01.json, reuse CaseState as if its rules were generic, or assume IntroState supplies episode outcomes or persistence. Shared readers/modal patterns may be reused only after checking their coupling. Report current interface findings before authoring new code. Final resident models, real radio, actual camera archives, and a directed-scene framework are unnecessary for a text-first slice.

## Bounded implementation sequence

| Unit | One outcome | Depends on | Check to implement |
|---|---|---|---|
| RR-1 | Load the three source records and exact speaker-labelled text independently | Authorized native content issue | EditMode CommonHouse.Tests.RRContentTests |
| RR-2 | Rehearse source acquisition, comparison, scope confirmation, deferral, and both traces | RR-1; current source and privacy review | EditMode CommonHouse.Tests.RRRehearsalTests |
| RR-3 | Play S00–S02 with inspection, readable replay, and recoverable technical consultation | RR-1 and RR-2; chosen local targets | PlayMode CommonHouse.Tests.RROpeningTests |
| RR-4 | Play both complete routes with private conversation and branch-specific aftermath | RR-3; actual listener/confirmation integration | PlayMode CommonHouse.Tests.RRRoutesTests |
| RR-5 | Persist and reload only this episode's state without repeated effects | RR-4; separate versioned save schema | PlayMode CommonHouse.Tests.RRReloadTests |

All five fixtures are future deliverables, not existing commands that already pass. The native runner exists and requires the pinned Editor, normal licensing, real result XML, and nonzero matching tests.

## First native issue draft

### Goal

Load Read receipt's three source records, scene/line IDs, and full accessible text as an isolated authored content asset.

### Context

Owning sources are this package's script and contract. Source namespace is RR; first slice is S00–S02, not a full case. Dependencies on new shared interfaces must be actual native issue links, discovered from current code. Current writing issue #99 provides the draft only.

### Acceptance criteria

- Every declared source, scene, line, choice, and outcome reference resolves with duplicate and missing references rejected.
- Source origin, stored/live status, coverage, recipient scope, and meaningful captions are preserved exactly.
- Loading grants no acquisition, interpretation, permission, outcome, or outgoing event.
- Case 01 and intro content still load independently with their existing IDs unchanged.

### Test command

After implementing the fixture, run `python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.RRContentTests`. Missing or zero matching tests fail. Until that fixture exists, only `python3 scripts/check_docs.py` checks this document package, and it does not qualify the runtime.

### Out of scope

Scenes, saves, actor animations, final assets, network services, automatic episode starts, new world-rule explanations, broad framework refactoring, and Open Pilot assignment. Create a native issue and inspect its prerequisites before implementation.

## Evidence and handoff

Return actual changed behavior, source revision and dirty-source disclosure, focused counts, baseline effects, and limits. Scene routes, graphical rendering, desktop builds, and human observation remain separate evidence. Carry Read receipt's branch-specific trace and private knowledge into later work; never assume a completed earlier episode or canonical outcome.
