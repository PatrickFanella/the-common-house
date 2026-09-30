# Season 1 writing review

Writing scope: [issue #99](https://git.subcult.tv/PatrickFanella/the-common-house/issues/99). Review performed on 2026-09-29 in the active T3 worktree. The owner authorized creative decisions; new material remains a working author draft. Start with the [roadmap](../design/season-01-story-roadmap.md), [writers bible](../design/season-01-writers-bible.md), and [transition scenes](../design/season-01-transition-scenes.md).

## Delivered text

Motion detected is assigned Episode 1 without changing its script or factual contract. Episodes 2–8 and the finale each have a new brief, script, contract, and agent handoff. The eight new packages contain 48 scenes, 104 numbered spoken lines, 24 source records with exact text or descriptions, 24 choice options including recoverable deferral, and 16 closing outcomes. Nine domestic bridges include the proposed intro connector.

The files are text-first treatments. They do not contain final shot lists, complete prop blocking, recorded performances, implemented runtime, or qualified episode builds. Future handoffs explicitly identify their fixtures as deliverables, not existing passing tests.

## Checks and corrections

`python3 scripts/check_docs.py` passed after the new packages and links were added. A separate one-off structural inspection checked each new package for S00–S05, unique numbered dialogue IDs, three uniquely named sources, three choice options, two defined outcomes, matching source references in its contract, and its handoff file. It reported the counts above. This inspection is not a reusable game-content validator.

The manual contract pass checked both closing acts against original signal coverage, factual supports, audiences, current permission, deferral, and later acknowledgment. It corrected:

- A duplicate cast label. The new news presenter is Bex, distinct from Case 01's caller Ada.
- Additional listeners in personal conversations. Jo or Ren joins only by explicit invitation; the private variant remains available.
- An anticipatory finale clue. CC-T02 is generated after the approved contact test, never acquired before the choice that creates it. A failed or postponed test does not manufacture the success transcript.
- Transport clues lacking observable text. NM-14 is now written in specific modern wrappers and the diagnostic manifest, separate from original authorship and dates.
- A disconnected-endpoint continuity risk. Orchard can forward the finale proposal through ordinary radio; reading it does not silently reconnect a process.
- Stale intro handoff status. Its foundations are recorded as merged through PR #96; full presentation and persistence remain separately scoped.

Existing Motion detected script and case contract, Case01.json, Case 01 source/state, and episode runtime are unchanged. The owner-selected son-return reveal and unknown later fate remain intact. New author direction for the intelligence is private and reviewable; it does not give characters knowledge of the secret project.

## Remaining review

Read dialogue aloud and assess whether the character voices hold across longer scenes. Review the proposed credit, privacy, and contact branches for felt consequence. Test concrete staging and recipient visibility when implemented. Decide progression rules before a coordinator assumes completed episodes. Final names, assets, dates, room privacy, and performances remain production decisions.

No claim is made about a human emotional response, Unity compilation, graphics, saves, desktop builds, or hosted runtime checks from this writing pass. The separate surface-shader PR owns its own graphical evidence. Unrelated live Editor settings remain outside the writing commit.
