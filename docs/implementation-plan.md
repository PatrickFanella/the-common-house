# Case 01 implementation plan

The owner selected **The Road Is Clear** as the working POC baseline on
2026-09-24. Names and story details remain provisional. The aim is one
complete investigation that changes how the player understands and treats a
resident, followed by a decision about whether to continue or revise the concept.
There is no release schedule or commercial scope in this plan.

[Private Gitea repository](https://git.subcult.tv/PatrickFanella/the-common-house) ·
[Issue tracker](https://git.subcult.tv/PatrickFanella/the-common-house/issues) ·
[Milestones](https://git.subcult.tv/PatrickFanella/the-common-house/milestones)

## Current evidence and boundaries

The inspected revision `12010f0` contains the design pack, with no Unity project,
Editor pin, package manifest, game code, or game tests. The T3 recipe serves
document source and validates local links. Its passing checks do not prove
Unity or gameplay behavior. M0 now provides a generated Unity project, portable document checker,
readiness inventory, causal contract, and local import/test runners on the
implementation branch. See [M0 verification](verification/milestone-0.md) for
the evidence and review status. No gameplay or player build is implied.

The plan follows the [decision register](design/decisions-and-open-questions.md),
[case truth and branches](design/case-01-the-road-is-clear.md),
[system contracts](design/radio-and-investigation-systems.md),
[technical outline](design/technical-design-outline.md),
[accessibility direction](design/art-audio-and-ux-direction.md), and
[POC evaluation plan](design/proof-of-concept-plan.md).

The baseline is three rooms, two residents, one caller, six evidence records,
one understandable privacy-routing inspection, three resolutions, and their
domestic aftermaths. The proposed 20–30-minute duration must be measured.
Temporary visuals and text-delivered dialogue are sufficient.

The optional memory, final anomalous receiver beat, final art/voices, external
services, elaborate radio simulation, and release planning remain outside this
first backlog. The final beat remains proposed in the case document; evaluate
its value after the ordinary human case works before creating a separate task.

## Implementation order

1. **M0 — Case contract and Unity foundations.** Add portable document checking,
   inspect the actual desktop toolchain, write the causal/branch contract,
   generate the real project, and establish a trustworthy focused test runner.
2. **M1 — Narrative rehearsal.** Encode the six evidence records, idempotent
   state transitions, consent rules, and readable runs through all outcomes.
   This checks the causal structure before scene presentation obscures defects.
3. **M2 — Playable outpost.** Build the blockout and small reusable interactions,
   then wire the opening, individual clues, consent conversation, routing, and
   aftermath. Foundational UI tasks include keyboard controls from the start.
4. **M3 — Verifiable local POC.** Add versioned saves and reload integration,
   verify whole-route keyboard access and text scaling, provide optional hints,
   and qualify real scene routes plus one local desktop build. Prepare the
   observation worksheet without fabricating participant results.
5. **M4 — Concept evaluation.** Analyze actual owner-supplied observations and
   recommend continuation, narrowing, or a specific revision for owner review.

Milestones are outcome groups, not deadlines. Each issue is one deliverable
with its own verification and exclusions. Native Gitea dependency links are
installed; the table below is the initial sequence, while issue state and
subsequent scope changes belong in Gitea.

## Atomic task backlog

| Issue | One deliverable | Blocked by |
|---|---|---|
| [#1](https://git.subcult.tv/PatrickFanella/the-common-house/issues/1) | Add a portable document validation command | Starting task |
| [#2](https://git.subcult.tv/PatrickFanella/the-common-house/issues/2) | Record the actual Unity development prerequisites | Starting task |
| [#3](https://git.subcult.tv/PatrickFanella/the-common-house/issues/3) | Write the Case 01 causal and branch contract | [#1](https://git.subcult.tv/PatrickFanella/the-common-house/issues/1) |
| [#4](https://git.subcult.tv/PatrickFanella/the-common-house/issues/4) | Create the minimal Unity project from the verified Editor | [#2](https://git.subcult.tv/PatrickFanella/the-common-house/issues/2); external prerequisite in issue |
| [#5](https://git.subcult.tv/PatrickFanella/the-common-house/issues/5) | Add a deterministic Unity test runner | [#4](https://git.subcult.tv/PatrickFanella/the-common-house/issues/4) |
| [#6](https://git.subcult.tv/PatrickFanella/the-common-house/issues/6) | Define and validate the six Case 01 evidence records | [#3](https://git.subcult.tv/PatrickFanella/the-common-house/issues/3), [#5](https://git.subcult.tv/PatrickFanella/the-common-house/issues/5) |
| [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7) | Implement idempotent case state transitions | [#6](https://git.subcult.tv/PatrickFanella/the-common-house/issues/6) |
| [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8) | Implement consent and resolution eligibility rules | [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7) |
| [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9) | Add a text-only rehearsal of all Case 01 routes | [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8) |
| [#10](https://git.subcult.tv/PatrickFanella/the-common-house/issues/10) | Block out the three connected outpost rooms | [#5](https://git.subcult.tv/PatrickFanella/the-common-house/issues/5) |
| [#11](https://git.subcult.tv/PatrickFanella/the-common-house/issues/11) | Add a first-person movement controller | [#10](https://git.subcult.tv/PatrickFanella/the-common-house/issues/10) |
| [#12](https://git.subcult.tv/PatrickFanella/the-common-house/issues/12) | Add a reusable inspect interaction | [#11](https://git.subcult.tv/PatrickFanella/the-common-house/issues/11) |
| [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13) | Add the replayable radio transmission log | [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7), [#12](https://git.subcult.tv/PatrickFanella/the-common-house/issues/12) |
| [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14) | Add the evidence notebook | [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7), [#12](https://git.subcult.tv/PatrickFanella/the-common-house/issues/12) |
| [#15](https://git.subcult.tv/PatrickFanella/the-common-house/issues/15) | Add side-by-side evidence comparison | [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14) |
| [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16) | Add the resident dialogue presenter | [#7](https://git.subcult.tv/PatrickFanella/the-common-house/issues/7), [#12](https://git.subcult.tv/PatrickFanella/the-common-house/issues/12) |
| [#17](https://git.subcult.tv/PatrickFanella/the-common-house/issues/17) | Stage the opening household care scene | [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13), [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16) |
| [#18](https://git.subcult.tv/PatrickFanella/the-common-house/issues/18) | Wire the replayable archive tape evidence E02 | [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13), [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14), [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16), [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9) |
| [#19](https://git.subcult.tv/PatrickFanella/the-common-house/issues/19) | Wire the roster and repair-note evidence E03 | [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14), [#15](https://git.subcult.tv/PatrickFanella/the-common-house/issues/15), [#17](https://git.subcult.tv/PatrickFanella/the-common-house/issues/17), [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9) |
| [#20](https://git.subcult.tv/PatrickFanella/the-common-house/issues/20) | Wire Ivo's missing-page evidence E05 | [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13), [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14), [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16), [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9) |
| [#21](https://git.subcult.tv/PatrickFanella/the-common-house/issues/21) | Wire the shared-repair corroboration evidence E06 | [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13), [#14](https://git.subcult.tv/PatrickFanella/the-common-house/issues/14), [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16), [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9) |
| [#22](https://git.subcult.tv/PatrickFanella/the-common-house/issues/22) | Implement Ren's private identity and consent conversation | [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8), [#16](https://git.subcult.tv/PatrickFanella/the-common-house/issues/16) |
| [#23](https://git.subcult.tv/PatrickFanella/the-common-house/issues/23) | Implement the private-route inspection interaction | [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8), [#12](https://git.subcult.tv/PatrickFanella/the-common-house/issues/12) |
| [#24](https://git.subcult.tv/PatrickFanella/the-common-house/issues/24) | Add the consequential send confirmation panel | [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13), [#21](https://git.subcult.tv/PatrickFanella/the-common-house/issues/21), [#22](https://git.subcult.tv/PatrickFanella/the-common-house/issues/22), [#23](https://git.subcult.tv/PatrickFanella/the-common-house/issues/23) |
| [#25](https://git.subcult.tv/PatrickFanella/the-common-house/issues/25) | Present the matching household aftermath | [#17](https://git.subcult.tv/PatrickFanella/the-common-house/issues/17), [#18](https://git.subcult.tv/PatrickFanella/the-common-house/issues/18), [#19](https://git.subcult.tv/PatrickFanella/the-common-house/issues/19), [#20](https://git.subcult.tv/PatrickFanella/the-common-house/issues/20), [#21](https://git.subcult.tv/PatrickFanella/the-common-house/issues/21), [#24](https://git.subcult.tv/PatrickFanella/the-common-house/issues/24) |
| [#26](https://git.subcult.tv/PatrickFanella/the-common-house/issues/26) | Add versioned case-state persistence | [#8](https://git.subcult.tv/PatrickFanella/the-common-house/issues/8) |
| [#27](https://git.subcult.tv/PatrickFanella/the-common-house/issues/27) | Integrate save and reload around the final send | [#25](https://git.subcult.tv/PatrickFanella/the-common-house/issues/25), [#26](https://git.subcult.tv/PatrickFanella/the-common-house/issues/26) |
| [#28](https://git.subcult.tv/PatrickFanella/the-common-house/issues/28) | Complete keyboard navigation for the core route | [#15](https://git.subcult.tv/PatrickFanella/the-common-house/issues/15), [#27](https://git.subcult.tv/PatrickFanella/the-common-house/issues/27) |
| [#29](https://git.subcult.tv/PatrickFanella/the-common-house/issues/29) | Add scalable case text | [#15](https://git.subcult.tv/PatrickFanella/the-common-house/issues/15), [#25](https://git.subcult.tv/PatrickFanella/the-common-house/issues/25) |
| [#30](https://git.subcult.tv/PatrickFanella/the-common-house/issues/30) | Add optional state-aware investigation hints | [#9](https://git.subcult.tv/PatrickFanella/the-common-house/issues/9), [#18](https://git.subcult.tv/PatrickFanella/the-common-house/issues/18), [#19](https://git.subcult.tv/PatrickFanella/the-common-house/issues/19), [#20](https://git.subcult.tv/PatrickFanella/the-common-house/issues/20), [#21](https://git.subcult.tv/PatrickFanella/the-common-house/issues/21), [#22](https://git.subcult.tv/PatrickFanella/the-common-house/issues/22) |
| [#31](https://git.subcult.tv/PatrickFanella/the-common-house/issues/31) | Qualify the complete Case 01 scene routes | [#27](https://git.subcult.tv/PatrickFanella/the-common-house/issues/27), [#28](https://git.subcult.tv/PatrickFanella/the-common-house/issues/28), [#29](https://git.subcult.tv/PatrickFanella/the-common-house/issues/29), [#30](https://git.subcult.tv/PatrickFanella/the-common-house/issues/30) |
| [#32](https://git.subcult.tv/PatrickFanella/the-common-house/issues/32) | Produce a reproducible local desktop POC build | [#31](https://git.subcult.tv/PatrickFanella/the-common-house/issues/31) |
| [#33](https://git.subcult.tv/PatrickFanella/the-common-house/issues/33) | Prepare the POC observation worksheet | [#3](https://git.subcult.tv/PatrickFanella/the-common-house/issues/3) |
| [#34](https://git.subcult.tv/PatrickFanella/the-common-house/issues/34) | Evaluate the first recorded POC playtest round | [#32](https://git.subcult.tv/PatrickFanella/the-common-house/issues/32), [#33](https://git.subcult.tv/PatrickFanella/the-common-house/issues/33); external prerequisite in issue |

## First useful work

Begin with #1 (portable document validation) and #2 (read-only Unity readiness).
They can be handled independently. #3 then resolves the written causal
contract. A blocked readiness report is a valid inspection deliverable but
does not permit #4 to fabricate an Editor version or bypass normal licensing.

After #5 establishes actual Unity verification, the narrative model and room
blockout can proceed independently. Avoid concurrent edits to the same Unity
scene or content asset; issue independence does not make Unity YAML conflicts
safe to resolve mechanically. Keep shared scene wiring sequential where needed.

## Verification contracts

Issue test commands are the commands each implementation must make meaningful.
The portable checker, Unity import checker, focused runner, and bootstrap
smoke fixtures are implemented in M0. Downstream gameplay fixtures and the
local build wrapper remain future issue deliverables. Downstream issues
depend on the relevant foundations being reviewed and integrated. Do not run a downstream task
and silently replace its unavailable check with a file-existence test.

The implemented runner interface is
`python3 scripts/run_unity_tests.py --platform editmode|playmode --filter NAME`.
The implementation must verify Editor identity, fail on zero selected tests,
preserve failure logs, and use disposable state. Actual compatibility, graphics
requirements, and licensing are established by #2–#5, not inferred from this
interface. The generated project now pins Unity 6000.4.10f1; the import and smoke
results apply to that installed Editor on Kvant.

Verification increases in scope as implementation proceeds:

- Pure case-state tests prove event idempotency, evidence order, recipient
  records, and resolution eligibility.
- PlayMode interaction tests prove the real scene interfaces invoke those
  rules, with explicit cancel, repeat, pause, and focus behavior.
- Integrated routes cover R1/R2/R3, archive-first and conversation-first
  discovery, defer/resume, pre/post-send reload, and transcript-only play.
- A real local desktop build must launch and complete its documented smoke
  route. This is separate from Editor tests or headless compilation.
- Human observation evaluates mystery comprehension, recipient awareness,
  household care, suspense, and self-insertion. Automated tests cannot prove
  those outcomes. Proposed five-person thresholds remain qualitative.

R1 must require a separate explicit consent event after corroboration and
private-route verification. R2 is a complete, valid outcome. R3 must be an
informed disclosure with visible recipients. A clue, replay, or reload must
never silently grant consent or duplicate a consequential event.

## Open Pilot issue and queue policy

The repository template follows the current Switchyard Open Pilot form:
**Goal, Context, Acceptance Criteria, Test Command, Out of Scope, Queue Checklist**.
Context names blocking issues and proposed file boundaries. Issue bodies
distinguish checks to implement from commands already available.

Open Pilot lifecycle labels were copied from the live Switchyard repository.
The starting tasks have `ready-for-agent`; dependent tasks have
`status:blocked`. No issue has `agent:queued` or the `open-pilot` assignee.
Bot assignment can trigger pickup just as labels can. This plan authorizes
tracking work, not starting the automation service.

Before queueing an issue, confirm its prerequisites are actually satisfied,
review its final body, verify the focused command in the intended execution
environment, and confirm repository and command admission through normal
Open Pilot policy. Unity support in the isolated Open Pilot worker has not
been qualified. A local licensed desktop can support agent-assisted work
without proving an unattended worker has the same capabilities.

The creation of labels and issues does not change Switchyard runtime policy,
licensing, secrets, merge permissions, or production services. Agent PRs must
retain their verification evidence and await the configured review process.
