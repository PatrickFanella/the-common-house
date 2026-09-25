---
title: Dialogue presenter
type: documentation
status: draft
visibility: private
---

# Dialogue presenter

`DialogueNode` records a speaker ID, an authored question, a response or
canonical response source, eligibility, and explicit effects. The presenter
shows only eligible nodes, labels each displayed answer with its speaker, and
uses fixed event IDs for effects. Repeated delivery therefore cannot duplicate
the private account disclosure, refusal authorization, or a promise.

The dialogue panel has no timer. Use `Up` and `Down` to choose a question,
`Enter` to ask it, and `Escape` to close through `ModalInputCoordinator`.
Closing has no state effect, so recovery leads remain available.

Dialogue opens only from a reachable resident through the shared `E` focus
interaction. `ResidentDialogueTarget` asks `DialogueCatalog` for a fresh node
list for that resident when the player interacts. There is no global dialogue
key, so private Ren conversation cannot be opened from another room. The
presenter uses its `CaseSession`'s current state, including a validated state
replacement after loading.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.DialoguePresenterTests
```

Focused tests cover eligibility, attribution, cancellation, keyboard selection,
and repeated effects. The panel is provisional text presentation, not final
character writing, a voiced protagonist, or a schedule system.
