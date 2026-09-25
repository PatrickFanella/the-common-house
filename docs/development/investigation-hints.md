---
title: Optional Investigation Hints
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# Optional Investigation Hints

Press `H` to request help. The first request shows an available lead, the
second compares currently recorded facts, and the third explains the applicable
route rule. Hints advance only on repeated player requests. A state change
returns the next request to the available-lead level. `Escape` closes the hint
panel through the shared modal coordinator.

Hints are read-only. They do not acquire sources, perform dialogue, verify a
route, send a response, or change identity or authorization. They never treat
E03's repair-mark similarity or E06's corroboration as identity confirmation or
private-contact consent. The authorized refusal remains a complete available
route while optional archive, roster, repair-note, and missing-log sources can
be investigated. A deferred case can be resumed with its recorded sources
intact. Resolved cases show no investigation hint.

Run the focused local check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.HintTests
```

The test covers staged requests, optional E03 handling, refusal and defer
states, E06/private-route consent boundaries, and resolved-state suppression.
