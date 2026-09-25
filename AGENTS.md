# The Common House agent instructions

Read `README.md`, `docs/development.md`, `docs/implementation-plan.md`, and
`docs/design/decisions-and-open-questions.md` before implementation.
`docs/design/` owns creative design; Gitea issues own task scope and status.

Work on one issue with one focused outcome. Read its native blocking issues
and body before editing. Do not treat a closed prerequisite as evidence that
an explicitly reported Editor, license, or owner-input blocker is resolved.
Preserve existing work and keep unrelated fixes in separate issues.

Case 01 is the working POC baseline. Names and detailed story choices remain
provisional. Preserve confirmed decisions, particularly explicit consent,
recipient visibility, transcript-equivalent clues, and a complete refusal
outcome. Do not add memory scenes, release plans, external services, or final
assets to the baseline without an explicit scope change.

Run the issue's focused check after implementing its prerequisites. Planned
paths and test commands are not existing functionality. Until the portable
checker is implemented, document validation uses the installed T3 recipe
as described in `docs/development.md`. Unity checks require the actual Editor
and normal licensing; missing tooling, missing results, and zero matching
tests must not be reported as passes. Keep scene checks, desktop build checks,
and human playtest evidence distinct. Use disposable test state.

Use `.gitea/ISSUE_TEMPLATE/open-pilot-task.yaml` for new tasks. Name dependencies,
one outcome, checkable acceptance criteria, one focused test command, and
exclusions. Do not add `agent:queued` or assign the automation account unless
the user has authorized execution. Labels alone do not qualify Unity support
in Open Pilot. Do not change runner policies or secrets to make a task pass.

The selected Gitea remote is private. Preserve that visibility and the current
license boundary; open source is a story theme, not a chosen distribution license.
