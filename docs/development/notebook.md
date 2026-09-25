---
title: Evidence Notebook
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# Evidence Notebook

Press `N` to open the notebook. Use Left/Right or Up/Down to select an
acquired source. `Escape` is handled by the shared modal coordinator and closes
the notebook before returning movement and pointer focus.

The notebook is read-only. It draws from the current scene's shared
`CaseSession`, so acquired sources remain available after returning from another
room. Opening, selecting, and reading an entry do not acquire evidence, change
authorization, or make an inference.

Each entry shows the original acquired source text, its caption when supplied,
and the source's authored attribution. Evidence statements retain their authored
labels: Observation, Attributed claim, Hypothesis, or Corroboration. A composite
record shows how many of its sources have been acquired. Its statement appears
only after all of that statement's sources are known; a missing source's text,
speaker, and future statement remain hidden.

The temporary IMGUI reader wraps and scrolls long text for the current small
GameView. It is a local blockout presentation, not a final UI or a claim of
screen-reader support.

Run the focused local check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.NotebookTests
```

The test covers acquired-only entries, composite partial attribution, statement
labels, shared-session revisit behavior, keyboard selection, and modal ownership.
