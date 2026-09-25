---
title: Evidence Comparison
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# Evidence Comparison

Open the evidence notebook with `N`, select an acquired source with Left/Right
or Up/Down, then press `C` to open a two-column comparison. `Tab` chooses the
left or right column. `Enter` puts the currently selected acquired source in
that column and replaces its previous selection. The same source may appear in
both columns when the player chooses it. Press `C` or `Escape` to return to the
notebook; pressing `Escape` again closes the notebook and restores movement and
pointer focus through the shared modal coordinator.

Each compared source retains its original source ID, authored attribution,
original text, caption, and available recorded-time metadata. Content without
an authored time displays `Time not recorded`; the POC does not invent calendar
times. The Case 01 archive tape is authored as `Dated before the current call`.

Comparison is read-only presentation. It accepts only entries already acquired
in the shared `CaseSession`, never acquires a source, and never changes identity
confirmation or consent. Comparing the two E03 documents therefore remains a
player-directed reading action rather than an inference or authorization step.

The temporary IMGUI columns wrap and scroll text for the small local GameView.
They are blockout UI, not a final UI or a claim of screen-reader support.

Run the focused local check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.EvidenceComparisonTests
```

The test covers keyboard placement and replacement, identical selections,
unavailable sources, source-time rendering, modal cancellation, and unchanged
case state.
