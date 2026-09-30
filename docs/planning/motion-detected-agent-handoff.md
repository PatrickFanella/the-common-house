---
title: Motion detected: implementation handoff
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Motion detected: implementation handoff

[Episode package](../design/episodes/motion-detected/README.md) · Planning only. No implementation, hosted issue creation, execution queue, or new test fixture is claimed here.

## Copyable agent assignment

> Read AGENTS.md and its referenced instructions, then docs/design/episodes/motion-detected/README.md, script.md, and case-contract.md. The script owns lines; the contract owns behavior. Work on the single authorized implementation issue and its native blockers. Reinspect current interfaces before editing. Preserve Case 01 and use an isolated episode scene and disposable save state. Implement only that issue's outcome. List unresolved authoring decisions rather than inventing extra clips, endings, AI dialogue, or past scenes. Implement and run its named focused check; missing fixtures, Editor results, or zero matching tests are failures, not passes. Report content/model, scene, build, and human evidence separately.

Attach the specific issue body below or a refined descendant. Do not give an agent the entire episode as one unbounded task.

## Source inspection at authoring time

Inspected 2026-09-29 in this worktree, not a promise about another active branch:

- `Assets/CommonHouse/Content/CaseContent.cs` loads `Case01` and validates exactly the baseline case ID, cast, sources, and outcomes. The episode is not a drop-in JSON replacement.
- `Assets/CommonHouse/Dialogue/DialogueNode.cs` contains Case 01 effect kinds and eligibility based on `CaseState`. `DialogueCatalog.cs` routes only Ren and Ivo.
- `Assets/CommonHouse/Domain/CaseState.cs` implements baseline evidence, events, disclosure, and resolution; `Runtime/CaseSession.cs` loads the baseline by default.
- `Persistence/CaseSaveStore.cs` checks content identity; existing player saves must remain isolated.
- Existing interaction/modal, notebook, dialogue, and aftermath behavior offer patterns for accessibility and event handling. Reuse appropriate seams after checking coupling rather than making Jo an alias for Ren.
- No cutscene/Timeline/VideoPlayer implementation was found under `Assets/CommonHouse` in this inspection. Text-first directed playback and static frames are sufficient for the first slice. Real clip playback is a separate feature if later selected.

## Ordered work units

Proposed boundaries, not files already implemented. Each unit needs its own scoped issue in the repository template, native blocker links, and executable admission review before work starts.

| Unit | One outcome | Dependencies | Proposed boundary | Focused check to implement |
|---|---|---|---|---|
| MD-1 | Load an isolated opening-content asset with stable script/source IDs and accessible descriptions | Review opening script and authorized issue | New episode content/model and tests; preserve baseline loader | EditMode `CommonHouse.Tests.MotionDetectedContentTests` |
| MD-2 | Rehearse the complete episode's facts, choices, listeners, and outcomes without scene rendering | MD-1, reviewed complete contract | Episode-specific domain/state plus content for later scenes | EditMode `CommonHouse.Tests.MotionDetectedRehearsalTests` |
| MD-3 | Play MD-S00 through MD-S02 in an isolated test scene | MD-1; episode session seam established | New demo scene, receiver text/static-frame presenter, Jo/Min interactions | PlayMode `CommonHouse.Tests.MotionDetectedOpeningTests` |
| MD-4 | Play the full investigation and both outcomes with recovery | MD-2, MD-3 | Private conversation, comparison, confirmation, and aftermath integration | PlayMode `CommonHouse.Tests.MotionDetectedRoutesTests` |
| MD-5 | Save/reload the isolated episode without affecting baseline saves | MD-2, MD-4 | Episode-specific persistence and identity checks | EditMode `CommonHouse.Tests.MotionDetectedSaveTests` |
| MD-6 | Qualify an isolated desktop build and collect owner observation | MD-4, MD-5 | Build entry selection and retained evidence; no replacement of owner's usual build | Build command must be selected from current tooling, then actual player route and human observation |

MD-1/MD-2 should establish an episode-specific content/session seam. Decide bounded reuse versus a separate implementation from current code evidence; this plan does not authorize a broad framework refactor. New cinematics, voice work, realistic camera assets, and full video playback are later presentation tasks, not prerequisites to a text/static-frame playable slice.

## First issue draft, MD-1

### Goal

Load the Motion detected opening content independently of Case 01 and validate its source/line IDs and accessible evidence descriptions.

### Context

Read the episode package and MD-S00–MD-S02 in the script. Use MD-V01, MD-C01, and MD-T01 from the contract. The complete episode files are context, not permission to implement later scenes. Native blocker links must be added when prerequisite issues exist. Owner review of the opening and an authorized execution environment are admission prerequisites.

Suggested new paths are an episode-specific authored-content file under `Assets/CommonHouse/Content/` and a corresponding content loader/test boundary. Select exact paths after current-source inspection. Use local fictional content with no new external package.

### Acceptance criteria

- [ ] Opening dialogue/source IDs, speaker attribution, media descriptions, and optional device timestamp are represented in authored data and match the owning script/contract.
- [ ] Loader rejects duplicate IDs, dangling references, missing essential text or descriptions, and missing speaker attribution.
- [ ] Reading content does not grant player evidence, permissions, or resolution.
- [ ] Baseline `CaseContent.LoadBaseline()` and its existing focused content checks continue to work with the original cast and six clues.
- [ ] A static description/transcript route exposes every MD-V01 observation without sound or rendered footage.
- [ ] New tests load actual authored opening content and exercise invalid-reference and missing-description failures.

### Test command

The MotionDetectedContentTests fixture does not exist yet. MD-1 must implement it before this command can pass:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.MotionDetectedContentTests
```

Also run the existing baseline content regression after changing any content seam:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseContentTests
```

### Out of scope

Full episode scenes, save migration, player expeditions, real video playback, Timeline, final art/audio, public broadcasts, new AI messages, and changes to Case 01 story or runtime behavior. Keep scope at authored opening data, its isolated loading seam, and focused validation.

### Queue checklist

- [ ] Owner has authorized this implementation issue.
- [ ] Opening treatment has been reviewed.
- [ ] Native blocking issue links and current execution-environment evidence have been added.
- [ ] Fixture will be implemented; the command must fail on missing tooling/results or zero tests.
- [ ] No automation account assignment or `agent:queued` label is applied until explicitly authorized.

## Later unit acceptance anchors

MD-2 must exercise both outcomes, partial evidence, source-order variation, cancellation, and replay idempotency. MD-3 must make opening lines, source text, pause/skip/close, and keyboard interaction recoverable. MD-4 must enforce private listeners and current authorization at confirmation, including cancel and quiet closure without confession. MD-5 must test cross-case save rejection and exactly-once effects around reload. MD-6 needs actual desktop-player proof and separate owner observation; automated routes cannot qualify grief, guilt, or mystery comprehension.

## Documentation validation now

```sh
python3 scripts/check_docs.py docs/design/episodes/motion-detected/README.md docs/design/episodes/motion-detected/script.md docs/design/episodes/motion-detected/case-contract.md docs/planning/motion-detected-agent-handoff.md
```

This checks Markdown and local links. It does not execute the proposed episode fixtures or prove story coherence. The current documentation task stops with a reviewable package and handoff.
