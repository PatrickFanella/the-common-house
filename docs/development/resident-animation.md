# Resident animation

Issue [#85](https://git.subcult.tv/PatrickFanella/the-common-house/issues/85)
connects the existing Mixamo clips to the two temporary resident rigs.
Regenerate the scene with `python3 scripts/build_outpost.py`.

Each `ResidentMotion` has a standing position, nearby chair position, and a
short straight route between them. Ivo waits 20 seconds before walking; Ren
waits 32. Both sit for 24 seconds before standing and walking back. The routes
stay in their current rooms, with Ren inside the archive alcove. These are
placeholder household motions, not character schedules or navigation.

`start-walking`, `walking`, and `stop-walking` accompany explicit movement at
0.65 metres per second. The importer extracts horizontal root motion, and
`applyRootMotion` stays disabled so the clip cannot move the skeleton away
from the scripted position. Vertical posture stays in the pose for sitting.
The two resident Animators keep evaluating while out of view. `stand-to-sit` and
`sit-to-stand` play once; `sitting-idle` loops. A resident yields when the
player is within 0.75 metres of the next movement position. The collider and
E interaction target follow the body, with a lower target while seated.

A world conversation records its resident in `DialoguePresenter`. Selecting
a response plays `talking` only for that resident, or `sitting-talking` if
seated. Reading the displayed response keeps that gesture looping; there is
no voice timing or lip animation. Browsing questions before the first response
keeps the resident listening. Escape, switching residents, and the existing
load reset clear the prior talking state.

Modal panels and Escape pause hold the routine clock and position. Talking
continues while its response is displayed. Sitting or standing transitions
freeze in their current pose during a panel and finish after it closes; they
do not switch to a talking pose halfway through. Closing a conversation
resumes an interrupted walking clip at its prior time. Routine positions are
scene presentation state and restart when the scene is loaded; they are not
stored in case saves and never grant evidence or consent.

Run the focused native scene check:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ResidentAnimationTests
```

Add `--graphics --timeout 600` to retain rendered walking, seated, standing
response, and seated response PNGs in the run's private `test-data` directory.
Headless checks exercise the rigs and clips but produce no rendered evidence.
The runner uses disposable project and save state with the normal local
Editor license. Local checks do not qualify an Open Pilot worker.

See the [local verification report](../verification/resident-motion.md) for
results, inspected captures, build recovery, and remaining evidence limits.
