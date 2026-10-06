# Load key fallback verification

The owner reported that the game made sense but Load did not work, and
suspected another action used the key. On September 26, 2026 UTC, live
`hyprctl -j binds` output confirmed consuming, unmodified F9 press/release
bindings for dictation. The game accepted F9 for Load. Plain L had no desktop
binding or existing game action. Desktop settings and owner saves were not
changed.

Issue #50 adds L through the same validated Load path, while retaining F9.
The welcome/pause reference and development HUD show L. This establishes a
key conflict and an alternative binding; it does not retrospectively prove
the cause of every manual failure or a successful owner restoration.

The focused command passed 10/10 tests with Unity 6000.4.10f1:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.DecisionReloadTests
```

Evidence: `tests-playmode-20260926T011950Z-djy1_d6k` under
`~/.local/state/common-house-unity/`. The new cases dispatch L and F9 through
the handler called by native Update and restore a disposable saved clue.
An unrelated key preserves the active state. Existing tests cover validation
failure, modal reset, pre-send checkpoints, and completed outcomes.

The player smoke now invokes the L handler for its pre-send and completed-state
reloads. This is scripted dispatch in the actual player, not injected physical
keyboard input or a witnessed owner session.
