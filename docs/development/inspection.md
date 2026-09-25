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
`Rejected`; a rejected acquisition remains visible to the player. The generic controller retains a local ledger for isolated examples. The
outpost scene binds `CaseInspectionBinding` to the shared case session and
uses canonical D01/D02 targets. Opening either panel remains read-only; Enter
records its document through the case-aware ledger.

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
