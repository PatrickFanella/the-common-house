---
title: Response confirmation
type: documentation
status: draft
visibility: private
---

# Response confirmation

At the radio console, use the shared `E` prompt to prepare a response. Choose
the private headset, authorized refusal relay, or room-speaker disclosure with
the arrow keys, then press `Enter` to review the selected route.

The review panel displays the domain summary before any send: outcome target,
source, selected material, exact message text, and recipients. A blocked route
shows its current reason and cannot send. The public room-speaker route also
shows its unauthorized-disclosure warning. Press `A` to acknowledge that warning
before `Enter` can confirm the public disclosure.

`Escape` cancels the review without sending and retains the selected draft.
`D` defers the investigation while retaining the draft and leads. Reopen the
console and press `R` to resume the deferred draft and produce a fresh summary.
There is no global send key. The panel uses the shared modal owner, so only one
reader or confirmation panel can receive input at a time.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.SendConfirmationTests
```

The focused tests cover identity blocking, exact public and refusal summaries,
public acknowledgment, cancellation, defer/resume, one disclosure after a
repeated submit, and the radio-console world target. They are scripted
interaction evidence, not a human playtest or network transmission.
