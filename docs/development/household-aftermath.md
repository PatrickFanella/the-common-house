---
title: Household aftermath
type: documentation
status: draft
visibility: private
---

# Household aftermath

A reachable kitchen check-in appears only after the current case has resolved. Opening it only presents the outcome-specific household consequence. The player must press `Enter` to acknowledge the check-in; this calls the one stable `CompleteAftermath` event. Escape closes the panel without changing case state.

- **R1:** Ren invites the player into a routine repair. The private call remains Ren’s to share, if they choose.
- **R2:** Ren appreciates the refusal being relayed. It is a complete resolution, and ordinary household life remains available.
- **R3:** Ren asks for distance. Ivo acknowledges that the disclosure repeated an earlier breach, without claiming that acknowledgment erases it.

The aftermath does not add sources, disclosures, rewards, a relationship score, or a new resolution. After acknowledgment it remains viewable, while the domain phase and existing disclosure record remain unchanged.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.AftermathTests
```
