---
title: Episode authoring system
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Episode authoring system

[Design index](README.md) · Reusable writing workflow. This is an authoring system made of documents, stable identifiers, templates, and review gates. It adds no gameplay runtime, generated-content service, or execution automation.

## Start with the human story

Use [episodic writing priorities](episodic-story-and-case-writing.md) and [world rules](incident-and-network-author-truth.md). Describe the person, their ordinary life, what they are dealing with, the incoming signal, a consequential choice, and the trace left in the house. Keep several possible outcomes alive while brainstorming. Separate owner selections from draft treatments and open questions.

A scene cannot settle a world rule by accident. Consult the private incident account when it depends on signal origins, AI access, travel, time, or cross-time interaction. Resolve the needed rule or choose a smaller story that does not depend on it.

## One package per episode

Create `docs/design/episodes/<slug>/README.md`, `script.md`, and `case-contract.md`, plus `docs/planning/<slug>-agent-handoff.md`. Use the templates below. Exact episode position and release format need not be decided to assign a slug.

| Template | Owns |
|---|---|
| [Episode brief](templates/episode-brief-template.md) | Emotional core, selected/proposed/open status, cast, and reading order |
| [Script](templates/episode-script-template.md) | Player-visible lines, interactions, source descriptions, and presentation direction |
| [Case contract](templates/episode-contract-template.md) | Author chronology, facts, evidence supports, state, permissions, choices, and recovery |
| [Agent handoff](templates/episode-handoff-template.md) | Source inspection, one-outcome units, dependencies, acceptance, and verification |

[Motion detected](episodes/motion-detected/README.md) is the first worked example. Copy the template structure, not its cast, grief story, or branch count. Remove sections that do not apply and write full readable text where they do. Fill every placeholder or explicitly list it as open before handoff.

## Writing sequence and completion gates

| Stage | Work | Done when |
|---|---|---|
| Seed | Identify emotional burden, ordinary pleasure/care, incoming signal, and possible trace | Owner has a concrete human story to select or redirect |
| Truth | Write actual event order separately from receipt order and character beliefs | Every essential inference has observable supports; unknowns are stated; selected reveal is explicit |
| Play | Map scenes and optional investigative order | Each scene has entry, player action, exit, and recovery; required clues cannot disappear |
| Lines | Draft the script with IDs, questions, answers, and source coverage | Every essential fact and outcome is reachable in exact text; staging never supplies hidden evidence |
| Branch review | Walk choices and their recipient/permission effects | Every outcome has its own acknowledgment; optional refusal/defer behavior is clear; no single canonical best path is implied |
| Owner review | Present remaining factual, tonal, and ethical choices | Selections are dated in the brief; unselected treatments remain proposals |
| Agent preparation | Inspect actual current code and write one-outcome implementation units | First issue has scope, native blockers where applicable, measurable acceptance, and a focused test to implement or run |
| Verification | Model checks, scene routes, player build, and human observation | Each claim has evidence at its own level; human understanding is never inferred from unit tests |

Drafting and review can repeat. A complete draft is not automatically approved implementation. An owner can authorize an isolated opening slice while later story decisions remain open, provided the handoff names that boundary.

## IDs and ownership

Choose a unique episode prefix, such as `MD`. Use `S` for scenes, `L` for lines, `Q` for choices, `V` for video sources, `T` for technical/transmission reports, `C` for attributed conversations, `E` for composite evidence, and `O` for outcomes. Other source types may use a defined prefix such as `D` for documents.

Examples are `MD-S03`, `MD-L050`, and `MD-Q02-A`. These identify authoring material; they are not assumed Unity enums or methods. Keep IDs stable during line edits. Retire IDs deliberately rather than reusing them for a different fact or consequence. New episodes get their own namespace; Case 01 IDs stay intact.

Keep exact dialogue in the script, causal predicates in the contract, and build sequence in the handoff. References can name IDs without copying all the text. Author chronology may reveal facts that the player never learns; an implementation must not automatically put them in the notebook.

## Playable versus directed scenes

The script specifies the player's control, not only a cinematic image. PLAY provides exploration; INTERACT opens an explicit action; MEDIA stays within its source; DIRECTED stages a short exchange; CHOICE names options and effects; AFTERMATH gives an observable trace.

For every directed moment, specify entry, movement/camera control, pause, skip, readable fallback, effect acknowledgment, and return to play. Start with present-day text/actor exchanges unless the story requires another presentation. An unrecorded past encounter may be testimony; it is not automatically a playable flashback or factual cutscene.

Use timing for performances, never to expire essential clues or force responses. Text-first prototyping can validate the story's routes before final animation, voice, and video. It does not prove the final performance works.

## Review checklist

- [ ] The person has a life beyond the secret or trauma.
- [ ] The chosen emotional change has a domestic trace.
- [ ] Every transmitted glimpse has a source and stays within its coverage.
- [ ] Original time, receipt time, author chronology, and attributed dates are distinct.
- [ ] Each inference says what its evidence establishes and leaves unknown.
- [ ] Player choice affects an observable act, expression, audience, or relationship; its category is stated.
- [ ] Actual listeners and scoped authorization are explicit where information is shared.
- [ ] Declining an optional account preserves a valid path; deferral remains recoverable.
- [ ] Replay, skip, cancellation, and reload cannot fabricate knowledge or duplicate effects.
- [ ] All essential facts and choices have text/caption equivalents.
- [ ] No actor direction forces an unchosen protagonist identity.
- [ ] All scene/source/line/choice/outcome references resolve within the package.
- [ ] No incomplete decision is silently delegated as permission to invent story canon.
- [ ] New work is isolated from Case 01 and active development unless a scoped issue expressly changes a shared boundary.

## Maintaining and delivering a package

Save episode drafts in their package, not only in chat. Record confirmed changes and material open questions in the brief. Update the owning file, then reconcile references. Add new world-level decisions to the register; ordinary line revisions need only the episode's revision notes. Apply unslop to prose and dialogue while preserving intended voice.

Run `python3 scripts/check_docs.py` and `git diff --check` for document edits. Manually walk all declared routes and IDs against the contract. The document checker validates nonempty Markdown and supported local file links; it does not validate IDs, branches, facts, or the story's emotional effect. A script/contract validator can be a separately scoped future tool if repeated manual failures justify it.

Use `.gitea/ISSUE_TEMPLATE/open-pilot-task.yaml` when turning a unit into an actual issue. Native issues own execution scope and status; the writing package is not a queue. Distinguish an existing focused command from a fixture an agent must first implement. Publication, implementation, and merging each follow the user's authorization.
