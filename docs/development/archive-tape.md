# Archive tape E02

The labelled tape shelf sits against the south wall of the archive/workshop.
Approach it and press E at the focus prompt to read/play T02. The same range
and occlusion check used by inspection dispatches this explicit world action.
The radio panel displays the complete transcript, its relative date, speaker
attribution, and kitchen-bell caption. No audio asset is required.

Playback receives T02 once through `TransmissionLog.Receive`. Escape returns
to the radio list and then closes it. R reopens the log from another room;
select T02 with arrows and Enter to replay it. The notebook preserves the
observation and limited corroboration, without confirming Ren/Mara identity
or granting contact authorization.

The source says the recording predates the current call. It supplies no
calendar date; do not invent one. All names and detailed story choices remain
provisional.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ArchiveTapeTests
```

The focused test loads the generated scene, positions the isolated player near
the shelf, dispatches the same E input frame, checks transcript/caption/state,
then replays from another room and from the shelf without duplicate effects.
It is scripted scene evidence, not a human playtest.
