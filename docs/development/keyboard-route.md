---
title: Keyboard route
type: documentation
status: draft
visibility: private
---

# Keyboard route

The essential Case 01 route has keyboard alternatives for movement, looking,
focused world actions, reader selection, routing, confirmation, save/load, and
pause. Use `WASD` to move. When no panel is open, use the arrow keys to look:
left and right turn; up and down pitch the view. The same arrows move the
selection inside an open panel because modal input pauses movement and looking.

Use the visible `E` prompt to open focused objects. `Enter` reads, asks, marks,
or confirms according to the panel. `Escape` first closes the deepest active
view and then returns movement focus. The notebook uses `N`, comparison uses
`C` and `Tab`, route verification uses `V`, and a public send needs `A` to
acknowledge its warning before a separate `Enter` confirmation. `D` defers a
response and `R` resumes a saved draft. The development save controls are `F5`
for save, `F8` for a checkpoint, and `F9` for load.

There are no timed choices, pitch-only clues, color-only decisions, or pointer
requirements in this route. Essential audio information remains available in
text and captions.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.KeyboardRouteTests
```

The focused test uses scripted controller input and focus placement inside the
generated scene. It proves the code path and state transitions without host
keyboard injection or a human accessibility evaluation. Rendering and panel
scrolling verification are separate checks.
