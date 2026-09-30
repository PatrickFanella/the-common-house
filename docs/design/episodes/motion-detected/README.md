---
title: Motion detected: episode authoring package
type: documentation
status: draft
created: 2026-09-29
updated: 2026-09-29
visibility: private
tags:
- project/the-common-house
- medium/game
---

# Motion detected: episode authoring package

[Design index](../../README.md) · [Reusable authoring workflow](../../episode-authoring-system.md) · Draft episode, working ID `motion-detected`. This is a future episode proposal, not a replacement for Case 01. Its ordinal position is undecided.

## How to use this package

A game script needs dialogue and a description of what the player can do around it. Read these documents together:

| Document | Reader and purpose |
|---|---|
| [Playable script](script.md) | Writer, designer, and scene implementer: exact draft lines, player actions, entry/exit conditions, directed moments, and aftermath |
| [Case contract](case-contract.md) | Writer and programmer: author truth, source limits, evidence, state, choices, recovery, and verification routes |
| [Agent handoff](../../../planning/motion-detected-agent-handoff.md) | Implementing agent: current-code limitations, bounded work units, acceptance criteria, and checks to implement |

The script owns spoken lines. The contract owns conditions and facts. The handoff owns build sequence. Stable IDs connect them; when revising a line, preserve its ID unless its role changes.

## Settled emotional core

The resident is a mother with an adult son whose addiction damaged their relationship. They were estranged; he stole from the family. She nevertheless left the back basement door unlocked. He sometimes stayed for days on an old couch or cot, with belongings in bags. Some visits lasted long enough to make sincere plans for a better future, then the pattern repeated.

On the last visit, she searched his bags for missing money and set a boundary. She said something she regrets. He left, and she never saw him again. She does not know whether he is alive, died before the incident, or was caught in it. Her guilt is central, but does not establish that her boundary caused his disappearance.

Jo is the temporary name for the mother. Min is a temporary household resident with receiver skills. The son remains unnamed. Neither supporting resident is silently substituted for Ren or Ivo. Addiction does not exhaust the son's character; memories include ordinary competence, affection, and plans that mattered to him.

The particular last-exchange lines, a camera near the basement steps, and all episode staging are draft treatments. The old couch is the working prop choice; a cot remains compatible with the owner's correction. The last conversation is recounted by Jo. It is not assumed to have been recorded by the camera.

## Draft story question

Did her son try to return after their last encounter, and what can she do with an answer that still cannot tell her where he is now?

The initial signal is a fragment from a networked camera overlooking the back basement entrance. Its narrow view matters. Investigation happens through playback, source comparison, technical consultation, and a private conversation with Jo. The episode leaves a household trace instead of resolving the son's whole life.

## Review gates

The owner selected the reveal in which the clips prove her son returned and entered through the unlocked door, while his later fate remains unknown. Exact clip contents and scene treatment below are the draft realization of that decision.

Other open choices are final names, calendar dates, episode placement, AI involvement, final production assets, and performance timing. The son's present fate remains unknown in both proposed closing outcomes. This episode does not require establishing two-way contact with the past.

An agent can implement the isolated opening slice described in the handoff once that issue is authorized. Full episode implementation needs review of the draft script/contract and a scoped issue. This authoring request does not start development or queue automation.

## Presentation vocabulary

- **PLAY:** first-person exploration; no forced camera, timer, or automatic confession.
- **INTERACT:** explicit player action opens a readable, pausable conversation or source. Closing leaves it recoverable.
- **DIRECTED:** a short present-day exchange with defined start, skip, and exit behavior. It may use text-only presentation initially.
- **MEDIA:** a bounded source clip shown on equipment, with transcript and image/sound descriptions.
- **CHOICE:** exact player-facing options and their consequences.
- **AFTERMATH:** a visible or audible change that follows a completed outcome.

Directed scenes are optional presentation around explicit interactions, not a requirement to build a cinematic framework. Actor reactions and silence are direction, not player objectives. Proposed beat lengths guide performance; no essential information expires with elapsed time.

## Scope

Present-day action stays at the outpost. The former home appears only within camera coverage and Jo's attributed account. Baseline Case 01, its saves, and its assets remain independent. All transmissions are fictional authored material; no real camera, internet account, radio service, or family record is connected.

## Revision notes

2026-09-29: Captured the owner-selected mother/estranged adult-son history, boundary and guilt, basement couch or cot, and search through his bags. Owner selected camera proof of return through the unlocked door with later fate unknown. Added a draft playable script, source/state contract, and staged agent handoff. Exact dialogue, recording treatment, and closing branches remain reviewable proposals.
