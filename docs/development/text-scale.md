---
title: Scalable Case Text
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# Scalable Case Text

Press `-` or `=` while a case panel is open to set text size from 100% to 150%.
The setting covers dialogue, notebook comparisons, transcripts, inspection,
routing, send confirmation, opening, aftermath, and optional hints. Use PageUp,
PageDown, Home, and End to scroll long text; panel selection controls retain
their documented arrow keys.

The documented minimum supported GameView is 800×600. At 150%, panels use an
opaque background, wrapping, and bounded scroll regions; source labels, speaker
labels, and consequential captions remain in the readable content.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.TextScaleTests
```
