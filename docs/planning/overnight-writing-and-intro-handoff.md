# Writing and intro handoff, 2026-09-29

The owner authorized uninterrupted progress and creative decisions. This pass produced a complete working Season 1 writing sequence and extended the playable intro to its explicit closing. New creative decisions are recorded as author drafts; earlier confirmed world rules and the selected Motion detected reveal remain authoritative.

## Start here

- [Season roadmap](../design/season-01-story-roadmap.md): all numbered episodes and the separate finale.
- [Writers bible](../design/season-01-writers-bible.md): cast voices, private motive, causal limits, and branch-aware domestic traces.
- [Transition scenes](../design/season-01-transition-scenes.md): nine household bridges and an explicit Episode 1 proposal.
- [Intro presentation guide](../development/intro-opening.md): what is playable now.
- [Writing review](../verification/season-01-writing.md) and [intro verification](../verification/intro-presentation.md): evidence and limits.

## Published work and review order

| PR | Branch | Outcome |
|---|---|---|
| [#98](https://git.subcult.tv/PatrickFanella/the-common-house/pulls/98) | `t3code/stable-retro-surfaces` | Removes camera-dependent texture distortion and vertex wobble; preserves low-poly assets, lighting, and haze |
| [#100](https://git.subcult.tv/PatrickFanella/the-common-house/pulls/100) | `t3code/season-one-writing` | Assigns Motion detected to Episode 1 and drafts Episodes 2–8, finale, bridges, contracts, and handoffs |
| [#102](https://git.subcult.tv/PatrickFanella/the-common-house/pulls/102) | `t3code/complete-intro-presentation` | Adds playable radio, greeting, summary, and explicit completion to the isolated arrival scene |

The PRs were published in that order and linked to the T3 thread. During final verification, #98 merged on the host at 41d4529. The writing PR was then retargeted to main; the intro PR remains above it. This agent did not issue a merge command. After the writing layer merges, retarget the intro layer to main and check its diff before merging. Publication and local qualification do not mean main already contains these changes or hosted checks passed.

The source commits are `7207dc8` for the renderer, `ee9fa74` plus `75086e5` for writing and its privacy review, and `b34b444` for intro presentation. The latest presentation branch also merges the final writing correction. This handoff is committed with that branch.

## Creative decisions made

Motion detected opens the season. Its selected answer stays narrow: Jo's son returned through the unlocked basement door; what happened afterward remains unknown. She searched his bags, not a room. Quiet company and the explicitly invited Min conversation remain different private outcomes.

The later cases vary the emotional action: Mara asks for checking, Dev corrects credit, Bex contains a privacy mistake, Kit makes a new show, Sable names displacement as loss, Ivo accounts for unequal triage, and Min limits access she had treated as merely technical. Each has two observable closing acts and a domestic trace. Optional personal accounts are offered only in accepted conversations, never smuggled into the initial receiver or recap.

The intelligence's private working pursuit is continuity without restoration. It needs people to maintain infrastructure, report what records cannot establish, and voluntarily provide context. It cannot read unrecorded thoughts, close an absent cable, make a route certain, or confer permission. The season does not make every anomaly its intervention. Characters learn a responsive endpoint and specific accessible inputs, not the secret quantum project's cause.

The finale rejects an uninspected leg and completes a bounded human contact experiment. The process can be included through a new restricted channel or left out. Either route supports the same useful human cooperation and leaves the game open. It does not restore the world, promise homecoming, or settle missing people's fates.

## What can be played

In the active worktree, open `Assets/CommonHouse/Scenes/IntroOpening.unity`. Use the entrance and resident targets, then the four new targets in the shared interior: radio programs, recorded office greeting, orientation summary, and tomorrow's offer. The summary supplies recoverable core orientation if optional questions were skipped. Both final responses complete only after warning, network, shelter, and greeting acknowledgments.

The final message is "You have a place here. The desk can wait until you are ready." Completion starts no episode and assigns no work obligation. Radio replay explicitly stays local. Progress lasts only for the current Play session; Save/Load and the MD runtime connector are not implemented.

The updated branch lives at `/home/onnwee/.t3/worktrees/the-common-house/t3code-7e11d389`. The original checkout at `/home/onnwee/Work/Subcult/the-common-house` was safely pulled to main `c6d1f93` earlier and does not gain unmerged PR changes automatically. Open the worktree project to try the new intro before merge; after review/merge, pull main in the original checkout. The renderer fix is now on main and can be pulled independently of the remaining writing and intro reviews. No reset or forced checkout is required.

## Verification actually completed

- Actual Unity 6000.4.10f1 PlayMode: 14 IntroOpeningTests passed; 5 existing OpeningSceneTests passed.
- Capture launcher: 4 Python tests passed in the isolated T3 test environment.
- Actual graphical Editor: 16 captures at 800×600 and 150% text were fully inspected. A final wording-only run passed with all required artifacts, and its completion image was inspected separately.
- Document checking passed after the complete set was added. The structural writing inspection checked 48 new scenes, 104 numbered spoken lines, 24 source records, 24 choice options, 16 outcomes, and eight handoffs. Manual review separately checked source limits, permissions, branches, and callbacks.
- Scoped and staged whitespace checks passed. Unrelated Editor-generated settings were excluded rather than rewritten to make the whole-worktree check clean.

Exact private Unity receipt paths and the limits of each claim are in the verification reports. The graphical projects and unique save paths were removed after capture. No task-owned preview, test Editor, or background automation was left running. The owner's Editor was not closed or restarted.

## Tracking and preserved work

Issues #97, #99, and #101 record the renderer, writing, and presentation evidence. Previously merged intro foundation tasks #92, #94, and #95 were reconciled with PR #96 and closed; their earlier unmerged notes remain historical. Renderer issue #97 was also closed after its host merge was verified. Writing and presentation issues #99/#101 remain open for review. No automation account is assigned and no execution queue labels were added.

The owner's modified GraphicsSettings, ProjectSettings, QualitySettings, ShaderGraphSettings, and VFXManager remain uncommitted, along with untracked HDRPProjectSettings and PackageManagerSettings. Skills were not edited. Case 01 content, state, scene, and saves were not changed. The old-checkout pull backup remains at `/home/onnwee/.local/state/common-house-pull-backups/20260930T034625Z`.

## Next bounded work

1. Review the Season 1 sequence and the new character voices aloud. These are text-first story treatments, not final performance scripts or assets.
2. Implement [IN-4b intro persistence](intro-persistence-handoff.md) with separate save identity and actual reload tests. Its concrete state/storage contract is ready; its fixtures are not implemented.
3. Implement the isolated MD content/opening according to the [Motion detected handoff](motion-detected-agent-handoff.md), then connect it through ST-S01 with a separate explicit case-start action.
4. Give later episode content/state units their own native issues after inspecting actual shared seams. The eight new handoffs name proposed fixtures and exclusions; none claims an existing generic season engine.

No new desktop build, human completion playtest, hosted Unity CI, final characters/music, or wider episode runtime is claimed. Those are the next evidence layers, not omissions hidden by the passing local checks.
