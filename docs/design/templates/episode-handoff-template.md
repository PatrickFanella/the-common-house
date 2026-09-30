---
title: Episode agent handoff template
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Episode agent handoff template

[Authoring workflow](../episode-authoring-system.md) · Copy to `docs/planning/<slug>-agent-handoff.md`. Fix links after copying. Use the repository's native issue template when publishing tasks; this file does not queue work.

## Assignment

Read the project instructions and this episode's brief, script, and contract. Implement only the single authorized issue, after inspecting its native blockers and current code. Use episode-specific state. Preserve Case 01 and active work. Report unresolved author choices and separate model, scene, build, and human evidence.

## Current-code inspection

<Date, checkout, relevant actual interfaces, coupling, available presentation/saves, missing capabilities. Name what exists versus what requires implementation.>

## Ordered units

| Unit | One outcome | Dependencies | Proposed file boundary | Focused check existing/to implement |
|---|---|---|---|---|
| <unit> | <bounded deliverable> | <native blockers and creative prerequisites> | <paths> | <actual or future command> |

## First issue body

### Goal

<One outcome.>

### Context

<Owning script scenes/sources, current behavior, native blocking issue links, source boundaries, and owner-input dependencies.>

### Acceptance criteria

- [ ] <Observable expected behavior and boundary.>
- [ ] <Recovery/accessibility behavior.>
- [ ] <Compatibility with the existing baseline.>
- [ ] <Meaningful invalid-input or negative-path check.>

### Test command

<Name an existing focused command or explicitly name the fixture to implement first. Missing Editor/results and zero matching tests cannot pass. Add appropriate baseline regression only when shared seams change.>

### Out of scope

<Unrelated gameplay, world-rule inventions, final assets, services, broad refactoring, migration, and publication excluded from this unit.>

### Queue checklist

- [ ] Issue and prerequisites have been reviewed in the native host.
- [ ] Creative decisions needed for this unit are selected.
- [ ] Execution environment and command are qualified or explicitly named prerequisites.
- [ ] Execution is authorized before automation assignment or queue labels.

## Evidence to return

<Changed behavior, focused results, baseline effects, scene/build evidence if required, and remaining owner observation.>
