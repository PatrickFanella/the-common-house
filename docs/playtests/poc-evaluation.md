# Case 01 evaluation — initial owner exploration

Status: partial evidence for [issue #34](https://git.subcult.tv/PatrickFanella/the-common-house/issues/34).
Recorded on 2026-09-25 from the owner's voluntary reports in the development
conversation. This is one reported owner run, not a completed new-player round.
No third-party observations, recordings, or identifying participant data were supplied.

## Build and conditions

The conversation followed delivery of the [qualified Linux player](../verification/local-build.md)
from game-source commit `55ed4c425565fa5f44d78f96c499664b7a5b1b4e`, output-manifest
SHA-256 `ab3a03a4bf858d492e2a3131889cb9eb48b016da5132ffd929ea61f6ba8e338e`.
That is the intended artifact reference; the owner did not independently confirm
an executable path or hash. The session date, duration, display/input settings,
audio state, text scale, and save path were not reported.

There is one owner-reported exploratory run and no reported new-player sessions.
Assistance during play was not reported. Follow-up questions were asked afterward;
they establish the limited observations below, not an observed gameplay trace.

## Reported observations

| Topic | Owner report | Limit |
| --- | --- | --- |
| General operation | "I ran it it seemed to work" | No complete failure inventory or direct observation. |
| Outcome | "I did refusal" | R2 reported; exact route steps and aftermath interaction were not described. |
| Persistence | "I did save I didn't load" | Save was used; successful disk write and restoration were not verified by this report. |
| Clarity and attention | "it's a little hard to understand but I wasn't really trying to understand, just try things" | Retain the concern; the report cannot isolate presentation, story structure, or exploratory attention as its cause. |

No route failure was reported. This does not establish that no failure occurred.
Automated route/build results remain separate evidence and do not fill missing
human observations.

## Follow-up after controls guidance

After delivery of the controls-guidance build, the owner reported: "it makes
sense how it works" and "the load wasn't working, but I think it's because the
key is taken by something else". The executable and save root for this attempt
were not independently confirmed. This adds a positive self-report of general
understanding and a failed manual Load attempt; it does not establish recipient
prediction, inference reconstruction, or successful restoration.

Live desktop inspection confirmed unmodified F9 is consumed by push-to-talk
dictation, while the game used F9 for Load. This confirms a binding conflict,
not the complete cause of every observed load failure. [Issue #50](https://git.subcult.tv/PatrickFanella/the-common-house/issues/50)
adds L as a game-side alternative without changing desktop settings or saves.

## Comparison with the proposed qualitative triggers

The [POC plan](../design/proof-of-concept-plan.md) proposes a five-person round.
Its thresholds are unchanged. One owner exploration is not that sample.

| Proposed trigger | Current human evidence | Assessment |
| --- | --- | --- |
| Four of five explain the inference using two observations | No reconstruction or supporting observations reported | Not assessable |
| Four of five identify recipients before sending | No recipient expectation reported | Not assessable |
| Three of five recall care and its effect on their choice | No care recall or decision rationale reported | Not assessable |
| Every participant has a reachable completion path | One owner reports refusal; assistance and route details unknown | Limited R2 report; proposed-round result unavailable |
| Transcript-only play preserves essential evidence and decisions | Audio/transcript condition not reported | Not assessable |

Unknowns are not passes or failures. These notes support no statistical,
new-player usability, or audience-wide conclusion.

## Agent recommendation and owner direction

Agent recommendation: **narrow the next evaluation pass** to two observable
questions: does Load restore the intended saved state, and can the player explain
what a response sends and who will hear it before confirming? Record the actual
build, conditions, assistance, and any failure using the
[observation sheet](poc-observation-sheet.md). Keep this exploratory report even
if later sessions are more successful.

The qualified clarity concern also justifies inspecting first-run guidance for
concrete omissions. Any verified interface defect should receive its own atomic
issue and verification; this evaluation does not establish a need to rewrite the
case or add mechanics, rooms, memories, assets, or release scope.

Owner direction: "great, let's keep going" authorizes continued project work.
It is not recorded as approval of final story details, confirmation that the
qualitative thresholds passed, or a final creative evaluation decision.

Issue #34 remains open for a fuller evidence-backed concept evaluation. The
remaining input is actual observation of understanding and choice, including
new-player evidence if assessing the proposed round. No further detail is
inferred from what the owner was not watching during this exploratory run.
