---
title: Control guidance
type: documentation
status: draft
visibility: private
---

# Control guidance

The opening welcome names the existing keyboard controls before the waiting
call: movement and look, focused interaction, transcript review, notebook,
hint, development save/load, scrolling, text size, and Escape. Load is shown as
`L`; `F9` remains supported for desktops that do not reserve it. It contains no
case lead, route advice, recipient, or consent information.

When no panel owns input, `Escape` pauses movement and opens the same controls
reference. `Escape` resumes. If a reader or another modal panel is active,
Escape closes that panel first; it does not open a second controls modal. The
development save HUD stays hidden while paused, so resume with Escape before
checking a save result message.

The shared copy uses the normal wrapped and scrollable panel path. It is
intended to remain readable at the supported text scale with `PgUp`, `PgDn`,
`Home`, and `End` available for longer panels.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ControlGuidanceTests
```

The focused scene test verifies the welcome and pause read models, modal Escape
precedence, resume, HUD suppression, and unchanged case facts. Rendered
800x600 text-scale evidence and desktop build verification are separate checks,
recorded in the [verification report](../verification/control-guidance.md).

The [Load-key follow-up](../verification/load-key-fallback.md) records the F9
desktop conflict and L alternative.
