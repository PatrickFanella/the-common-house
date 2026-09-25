# Opening household care

The initial scene offers a small, provisional welcome at the radio desk before the first case call. The welcome says that food is available, explains that the transmission log supports replay, and describes the private-headset and room-speaker endpoint labels.

The welcome appears as a modal panel. Escape returns the player to the room. Dismissing or revisiting it does not create evidence, grant a reward, start a timer, or remove the first call. The first call is received only through its reachable radio-desk interaction target, which explicitly calls `TransmissionLog.Receive("T01")`. Repeated interaction leaves the existing received transmission unchanged.

`OpeningContent` holds this provisional non-evidence copy separately from Case 01 sources. It does not add protagonist history or make identity claims.

The opening offer and its presentation state are scene-local in this milestone. A future save/load issue must decide whether restoring in the opening scene should show the care panel again; it must never auto-receive T01.

Run the focused check with:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.OpeningSceneTests
```
