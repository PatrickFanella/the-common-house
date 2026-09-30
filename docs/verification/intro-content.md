# Intro content verification

Issue [#92](https://git.subcult.tv/PatrickFanella/the-common-house/issues/92), IN-1. Local qualification on Kvant, 2026-09-29 America/Chicago, using Unity 6000.4.10f1 in this thread's worktree. Source includes the intro authoring documents and implementation as uncommitted work.

## Implemented boundary

The actual `Intro` Unity TextAsset loads through `IntroContent.Load()` independently of Case 01. The content validator covers attribution, IDs, scene/source/choice references, required text and accessible descriptions, and the recorded office greeting. All 66 authored line texts were compared with the script; the former company name uses the explicit generic fallback.

No actor, scene, cutscene, player-state, save, or case-start integration is part of IN-1. Local content evidence does not qualify the complete intro, a player build, human comprehension, or an unattended Open Pilot worker.

## Focused results

The final `IntroContentTests` run passed 31 tests, including explicit dangling line, source, and choice references. Receipt directory: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T014059Z-2xpaykf3`.

The existing `CaseContentTests` regression passed 24 tests. Receipt directory: `/home/onnwee/.local/state/common-house-unity/tests-editmode-20260930T013823Z-3d02fyha`.

Document validation passed with 82 nonempty Markdown documents and 290 local file links. Tracked changes and new content, test, and documentation files passed whitespace checks. The resource inventory contains eight scenes, 66 lines, ten sources, eight menus, and nine speakers; all three new Unity assets have metadata.

The existing runner validates nonzero test-result XML and Editor logs, then removes the disposable project and unique save directory. Private receipts and source hashes remain on Kvant. Raw logs are not committed.
