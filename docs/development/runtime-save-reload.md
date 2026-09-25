---
title: Runtime development save and reload
type: documentation
status: draft
visibility: private
---

# Runtime development save and reload

`CaseRuntimeSaveController` is disabled until a caller explicitly supplies a `CaseSaveStore`, an explicit local development root, or opts the scene into its development environment. The environment path is mandatory: `COMMON_HOUSE_ENV=development` requires an absolute `COMMON_HOUSE_SAVE_PATH`. Isolated native tests use `Application.persistentDataPath/test-saves`; the native runner gives each copied project a unique company/product identity and removes that isolated save identity afterward. No normal user-save location is inferred. It does not load automatically, synchronize, or create save slots.

When configured for development, `F5` writes a manual development save, `F8` overwrites the same explicit development save with a pre-send checkpoint, and `F9` loads a validated save. The response confirmation uses the same checkpoint seam immediately before a final send. If that checkpoint fails, the response is not sent.

A load validates the file before changing the active session. It then closes and resets modal presentation derived from the old state, replaces `CaseSession.State`, and rebuilds notebook and transmission records. This clears stale confirmation summaries and public-disclosure acknowledgment, dialogue lines, route-page review, and panels. The loaded consent, recipients, draft/defer state, outcome, and aftermath marker remain domain facts from the save.

An invalid primary or backup reports the store error and leaves the running session unchanged. Tests inject a unique temporary root and remove it after each case.

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.DecisionReloadTests
```
