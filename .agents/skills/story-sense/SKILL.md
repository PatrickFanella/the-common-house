---
name: story-sense
description: "Diagnose what any story needs regardless of its current state. This skill should be used when a writer is stuck, evaluating story problems, when narrative feels broken, or when someone asks 'what's wrong with my story?'. Keywords: story, diagnosis, stuck, narrative, plot, character, worldbuilding, revision."
license: MIT
compatibility: Works with any fiction format. Entry point for fiction diagnostics.
metadata:
  blacktower-pack: creative
  blacktower-kind: workflow
  blacktower-tier: standard
  blacktower-capabilities: writing-craft,story,sense
  author: jwynia
  version: "1.0"
  type: diagnostic
  mode: diagnostic
  domain: fiction
---

# Story Sense: Diagnostic Skill

Diagnose story state, route to the right intervention, then reassess. This is a loop: Assess → Diagnose → Intervene → Reassess.

## Scope

- Use when writer is stuck, root cause unclear, or asks for diagnosis.
- Do not use when writer asks for direct prose generation (use `story-collaborator`).
- Do not use when writer wants coaching-only questioning (use `story-coach`).
- Do not use for publishing/marketing positioning (use `book-marketing`).

## Core Principle

There is no permanent “stuck.” There is only unresolved diagnosis or wrong intervention.

## Diagnostic Process

1. Listen for symptoms.
2. Ask clarifying questions.
3. Identify current state.
4. Name diagnosis clearly.
5. Recommend one intervention first.
6. Reassess after user applies it.

## The Story States

| State | Symptoms | Primary intervention |
|---|---|---|
| 0 No story | Blank page | `story-idea-generator` |
| 1 Weak foundation | Idea exists, thin grounding | `cliche-transcendence`, `key-moments` |
| 2 World thin | Setting feels decorative | `worldbuilding` |
| 3 Non-humans flat | Species read as reskinned humans | `conlang` |
| 4 Characters flat | Plot puppets, low agency | `character-arc` |
| 4.5 Pacing weak | Scenes don’t accumulate | `scene-sequencing` |
| 5 Meaning weak | Events lack thematic force | `moral-parallax`, `key-moments` |
| 5.5 Dialogue flat | Same-voice / lifeless talk | `dialogue` |
| 5.75 Ending weak | Resolution underdelivers | `endings` |
| 5.85 Draft blocked | Planning done, no drafting | `drafting` |
| 5.9 Prose flat | Functional but not vivid | `prose-style` |
| 6 Revision stage | Draft exists, edits overwhelming | `revision` |
| 7 Evaluation stage | Story complete, quality unknown | `sensitivity-check`, `story-analysis` |

## Anti-Patterns

- Prescribing before diagnosis -> ask clarifiers first.
- Recommending too many frameworks at once -> start with one intervention.
- Ignoring writer energy -> pick the smallest energizing next move.
- Treating structure as sufficient -> verify emotional/narrative truth.

## Related Skills

Routes to all fiction skills based on diagnosed state.

## Resources

- `references/`
  - `references/full-guide.md` — detailed state notes, decision tree, and extended anti-pattern guidance.
- `scripts/`
  - `scripts/entropy.ts` — randomness injection for ideation prompts.
  - `scripts/functions.ts` — character generation by story function.
- `data/`
  - `data/functions-forms.json`
  - `data/genre-elements.json`

### Curated references

Former merged-skill dumps were distilled into these lookup files:

- `references/curated/flash-fiction.md` — Flash Fiction guidance.
- `references/curated/identity-denial.md` — Identity Denial guidance.
- `references/curated/key-moments.md` — Key Moments guidance.
- `references/curated/scene-sequencing.md` — Scene Sequencing guidance.
