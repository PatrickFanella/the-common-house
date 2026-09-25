# Case 01 route qualification

`CaseRouteTests` qualifies six scene routes in the generated `OutpostBlockout`
scene: R1 private contact, R2 authorized refusal, and R3 informed public
disclosure, each in archive-first and conversation-first order. Every route
receives the opening call, reads the archive tape before or after Ren's private
account, replays that tape, uses the real focused world-target path, and reaches
one household aftermath.

R2 and R3 omit D01, D02, D03, and E06. This verifies that optional evidence is
not required for the authorized refusal or informed public-disclosure route. R1
adds Ada's repair follow-up, Ren's corroboration, route inspection, and Ren's
separate agreement. The tests assert that the archive, corroboration, and route
inspection never grant private-contact consent by themselves.

Each route defers and resumes its draft, reloads a unique disposable save before
sending, saves and reloads again after sending, and asserts exactly one outgoing
disclosure with its route-specific recipients. R3 also proves that confirmation
fails until the separate public-disclosure acknowledgment is supplied.

All six routes set `AudioListener.volume` to zero and restore its prior value in
cleanup. The tests obtain T01 and T02 through transcript text and assert T02's
`[Kitchen bell sounds on the recording.]` caption. There are no authored audio
assets or mixer controls in this POC, so this is muted transcript-and-caption
coverage rather than audio-device or mixer qualification.

Run the focused scene qualification from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.CaseRouteTests
```

The test supplies deterministic controller frames and places the scene camera in
the interaction lane before calling the real focus ray. It does not inject host
keyboard input, prove a human can navigate the physical route, replace desktop
build verification, or substitute for participant evaluation. The command
creates an isolated Unity project and each route removes its temporary case-save
directory. Record the source revision, Unity evidence directory, any failures,
and the remaining limits with the run that qualifies a change.
