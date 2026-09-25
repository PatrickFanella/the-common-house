---
title: Inspection interaction
type: documentation
status: draft
visibility: private
---

# Inspection interaction

`InspectionController` selects an `InspectionTarget` only when its collider is
inside the first-person camera's range and unobscured. The center prompt shows
`E` for inspection. While open, the text panel uses `Enter` to record an
explicit acquisition and `Escape` to close. Opening and rereading an item do
not record evidence.

The target owns its identifier, title, readable text, and optional acquisition
identifier. `IInspectionLedger` reports `Acquired`, `AlreadyAcquired`, or
`Rejected`; a rejected acquisition remains visible to the player. The generic
scene notice uses an in-memory ledger and no Case 01 identifier. Future case
wiring must provide a reviewed adapter rather than treating an open panel as
evidence acquisition.

`ModalInputCoordinator` owns `Escape` and the movement pause while a panel is
open. It permits one modal owner at a time and restores focus to the same
reachable target after close.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.InspectionTests
```

The text panel is built with Unity IMGUI and can be captured from the GameView.
Focused tests prove selection, occlusion, close, and idempotent acquisition;
they do not establish full keyboard accessibility or case-specific evidence
effects.
