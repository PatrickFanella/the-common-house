# Case 01 transmission log

Issue [#13](https://git.subcult.tv/PatrickFanella/the-common-house/issues/13)
adds a local, replayable log for acquired radio transmissions. It does not add
live radio, audio files, routing, speech recognition, or final voice work.

`TransmissionLog.Receive(sourceId)` is the explicit receive action. It accepts
only authored sources whose kind is `transmission`, records acquisition through
the shared case state, and uses a stable receive event ID. Opening the log,
changing selection, and reopening a transcript use `ReplaySource` only; they
do not acquire evidence, grant consent, or repeat disclosures.

For T01, the log presents Ada as the source speaker and “Mara” as Ada’s
asserted contact identity. That assertion remains distinct from the separate
Ren/Mara identity status. E04 can confirm Ren’s former name later; it does not
retroactively verify Ada’s caller identity or every claim.

The temporary IMGUI panel opens with `R`, supports Up/Down selection and Enter
to show a transcript, includes speaker labels and authored sound captions, and
uses the shared modal coordinator for Escape. Escape first returns from a
transcript to the list, then closes the log. The panel wraps and scrolls text
at small GameView sizes.

Run the focused native check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.TransmissionLogTests
```

The check covers state and modal behavior. It does not establish a human
usability review, player build, recorded audio, or hosted worker support.
