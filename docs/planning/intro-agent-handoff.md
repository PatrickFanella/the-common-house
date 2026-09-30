---
title: Intro: authoring and implementation handoff
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Intro: authoring and implementation handoff

[Intro package](../design/episodes/intro/README.md) · [Season 1 roadmap](../design/season-01-story-roadmap.md). IN-1 implementation is tracked in [issue #92](https://git.subcult.tv/PatrickFanella/the-common-house/issues/92); IN-2 in [issue #94](https://git.subcult.tv/PatrickFanella/the-common-house/issues/94). Later units remain planning. No automation is queued.

## Assignment for a future implementation issue

Read project instructions, the intro brief, script, and scene contract. Work on one authorized native issue after inspecting its body and blocking issues. Implement only the named intro slice. Preserve Case 01's opening, content, scene routes, and ordinary saves. New intro state must not trigger a case, grant consent, or replace the existing baseline automatically. Return focused evidence and clearly distinguish text/state checks, real scene behavior, build proof, and human observation.

## Current-code boundary

The repository has baseline household-opening, interaction, dialogue, radio-review, and modal patterns. They are not a complete arrival intro or a generic cinematic system. The last inspected content/dialogue/domain interfaces are Case 01-specific; see the [Motion detected handoff](motion-detected-agent-handoff.md) for the concrete source inventory. Reinspect the actual checkout before choosing new paths or shared seams.

No exterior entrance, actor travel, sleeping-space scene, sequential multi-episode coordinator, or directed-scene framework is assumed implemented. A text-first threshold opening can qualify the content/learning behavior without those capabilities. Final art, music, video, voice, and real radio services are unnecessary for that slice.

## Ordered units

| Unit | One outcome | Dependencies | Boundary | Check to implement |
|---|---|---|---|---|
| IN-1 | Independently load and validate intro text and source/scene references | Reviewed text and authorized issue | New intro content/model tests, baseline unchanged | EditMode `CommonHouse.Tests.IntroContentTests` |
| IN-2 | Rehearse orientation, optional expression, acknowledgment, and completion | IN-1, reviewed contract | Isolated intro state, no automatic Case 01 mutation | EditMode `CommonHouse.Tests.IntroRehearsalTests` |
| IN-3 | Play a threshold/reception/welcome slice with recovery | IN-1, IN-2, chosen scene boundary | Isolated scene/controller using appropriate shared UI seams | PlayMode `CommonHouse.Tests.IntroOpeningTests` |
| IN-4 | Play all intro scenes and preserve state across reload | IN-3, reviewed locations/branding | Full isolated intro integration and save identity | PlayMode `CommonHouse.Tests.IntroRoutesTests` |
| IN-5 | Connect completed intro to the selected Episode 1 through explicit action | Episode 1 selected and reviewed; IN-4 | Bounded episode-start connector | Fixture and command to choose once connector exists |

`IntroContentTests` and `IntroRehearsalTests` are implemented for IN-1 and IN-2. The other fixtures remain future deliverables; listing them does not establish passing checks. Actual Editor/licensing/result requirements remain in the development guide.

## First issue draft, IN-1

### Goal

Load the intro's authored lines and source descriptions independently of Case 01, with explicit validation of references and accessible text.

### Context

Owning sources are the intro script and scene contract. The optional preferred-address field, former company name, exact exterior geometry, and first-episode connector remain open. Represent an undecided address and generic local-service greeting without inventing final canon. Add native blocker links if this depends on shared content work; labels or closed issues do not substitute for reading reported blockers.

### Acceptance criteria

- [ ] Actual authored intro content represents IN-S00–IN-S07 and all player-visible lines/options in the current reviewed script.
- [ ] Every source/scene/line reference resolves, with missing/duplicate IDs rejected.
- [ ] Program sources and recorded greeting have speaker-labelled transcripts and meaningful descriptions.
- [ ] Loading content grants no player knowledge, identity, permission, or case-start effect.
- [ ] Original Case 01 content loads unchanged and its existing focused regression remains valid.
- [ ] The intro can be represented without final exterior assets, music, voice, real services, or a cinematic package.

### Test command

Run the implemented IN-1 fixture:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroContentTests
```

Run baseline content regression if any content seam changes:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseContentTests
```

### Out of scope

Scene geometry, actor animation, live services, episode numbering, AI revelation, Case 01 rewriting, save migration, automatic episode starts, and broad framework refactoring.

### Queue checklist

- [ ] User authorized implementation of this issue.
- [ ] Text for this slice was reviewed.
- [ ] Native prerequisites and execution environment were inspected.
- [ ] Focused fixture will be implemented and run; missing/zero results fail.
- [ ] Automation assignment/queue labels await explicit authorization.

## Current validation boundary

The [intro content guide](../development/intro-content.md) describes the implemented IN-1 asset/loader and its separate [verification report](../verification/intro-content.md). The [state guide](../development/intro-state.md) describes the IN-2 rehearsal with a separate [verification report](../verification/intro-state.md). Both are local, unmerged implementation in this worktree. No intro scene or persistence is implemented by these units. Owner observation remains necessary to assess welcome, orientation, and the first anomaly.
