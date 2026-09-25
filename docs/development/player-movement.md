---
title: First-Person Movement
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# First-Person Movement

The Case 01 outpost uses Unity's existing legacy Input Manager. It does not add
or migrate an input package.

| Control | Action |
| --- | --- |
| `W` / Up Arrow | Move forward |
| `S` / Down Arrow | Move backward |
| `A` / Left Arrow | Move left |
| `D` / Right Arrow | Move right |
| Mouse | Look |
| `Escape` | Open or close the pause/menu focus state |

Mouse sensitivity is a per-controller Inspector setting until an options menu
exists. It is expressed as degrees per legacy mouse-input axis. Movement uses
a `CharacterController`, so the generated outpost's colliders stop the player
at room and exterior boundaries.

Opening the pause/menu focus state releases the pointer and suppresses movement
and looking. Losing application focus opens that state. Closing it with
`Escape` resumes native input and recaptures the pointer. No head bob or camera
shake is enabled by default.

## Automated-test isolation

In Unity batch mode and when `COMMON_HOUSE_ENV=test`, the controller does not
read native input or capture the host pointer. PlayMode tests inject a local
input source and call a deterministic frame method instead. Those checks prove
controller movement, collisions, pause suppression, and generated-scene
traversal; they do not prove a human desktop-control experience.

Run the focused local check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.PlayerMovementTests
```

See [development environment](../development.md) for Unity licensing,
disposable test-state, and result-handling requirements.
