# Playable arrival opening

Issue [#95](https://git.subcult.tv/PatrickFanella/the-common-house/issues/95) implements IN-3 from the [intro handoff](../planning/intro-agent-handoff.md). The [content](intro-content.md) and [state rehearsal](intro-state.md) prerequisites were committed locally as `812c2a3`; native issues #92 and #94 remain open for review. The [intro script](../design/episodes/intro/script.md) and [scene contract](../design/episodes/intro/case-contract.md) own the narrative.

Open `Assets/CommonHouse/Scenes/IntroOpening.unity` in Unity 6000.4.10f1 and enter Play mode. This is a separate opening scene; it does not replace the Case 01 scene or change the ordinary POC build's entry point. Temporary geometry stages a threshold inside the refuge, a reception counter, and a welcome table. It offers no expedition into disrupted space. Materials and the retro rendering components reuse existing project assets. The signs and interaction objects are primitive placeholders, not final resident designs or company branding.

## Play the opening

Use W/A/S/D to move, mouse or arrows to look, and E while focusing a nearby target. The six targets are the entrance notice, Ren at the threshold, Ivo's reception ledger, a repeatable return warning, the service-area map, and food/bedding at the welcome table. Range and walls affect focus through the existing inspection ray. Walking past a target acquires nothing.

The panel uses Up/Down and Enter to choose actions, or mouse buttons through the same controller method. Choosing an expression presents its response without committing it. Choose **Acknowledge and return** to record the presented text. **Close for now**, or Escape, returns to movement and discards unacknowledged selections. Reopening retains acknowledged progress. **I need a minute** keeps the arrival exchange available; other acknowledged arrival responses let Ren offer the return conversation next.

Reception permits **Leave it blank for now**. **My name** leaves the address undecided in this slice and explains that the ledger can be filled later; there is no name-entry UI yet. Every shelter response preserves the offer of food and bedding. The complete warning remains explicitly reviewable at reception even if optional threshold questions were skipped.

**Pause** blocks option selection and acknowledgment. **Read transcript** keeps the current readable text available without applying learning or choosing skipped options. Resume before acknowledging. PgUp/PgDn, Home/End, and -/= use the shared scroll/text-size controls. Conversations pause movement through the existing modal owner and do not aim, move, or teleport the camera. Closing a panel releases that owner; another panel cannot acquire intro content behind it.

This slice implements IN-S00, IN-S01, IN-S02, IN-S03, and IN-S05. It stops before the radio programs, recorded greeting, closing exchange, full intro completion, persistence, and Episode 1 connector. No CaseSession or save controller is attached. Progress lasts for the current Play session only.

## Generation and checks

The explicit Editor menu **Common House → Generate Arrival Opening** generates only this arrival scene and appends its build-list entry while preserving the existing entries. Do not use the outpost generator to produce it. `IntroOpeningSceneGenerator.GenerateAndExport` additionally exports the scene, metadata, and build list to the native runner's private output directory for narrow copy-back. The implementation used a disposable Editor copy and copied only those three files into the worktree.

```sh
python3 scripts/run_editor_method.py CommonHouse.Editor.IntroOpeningSceneGenerator.GenerateAndExport
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.IntroOpeningTests
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.OpeningSceneTests
python3 scripts/capture_panels.py --intro --scale 1.5
```

The first command does not copy generated sources back by itself. Do not use the runner's broad `--copy-back` for this narrow task. Review and copy the exported scene, its metadata, and the build list only. The capture command runs a graphical Editor on the local display with disposable project/save state, validates seven screenshots and a receipt, and exits. Inspect the retained images before claiming visual evidence.

The capture launcher's standard-library regression command is:

```sh
rtk proxy ~/.local/bin/t3-dev --profile the-common-house test --command 'python3 -m unittest discover -s tests -p test_capture_panels.py'
```

Actual results and their limits are recorded in the [opening verification report](../verification/intro-opening.md). Automated input frames and scripted rendered captures are separate from human playtest evidence. This unit does not qualify a desktop build or hosted Unity worker.
