# Case 01 local persistence

Issue [#26](https://git.subcult.tv/PatrickFanella/the-common-house/issues/26)
stores `CaseState` as a versioned local development save. It has no cloud
sync, save-slot UI, or release migration policy.

`CaseSaveStore` requires the loaded `CaseContent` and an explicit development
root. Callers must choose that root; tests use a unique temporary directory.
The store writes `case-01.json` and keeps the previous primary as
`case-01.json.bak` when replacing an existing save.

The snapshot records acquired sources, promises, attributed disclosure
materials and recipients, authorization and its event ID, route inspection,
draft/defer state, outcome, aftermath phase, and the event payload ledger.
Loading validates the schema, case ID, content references, recipient and
material contracts, consent prerequisites, outcome facts, and ledger entries.
It clears the transient presented confirmation summary, so presentation must
show a new summary after loading.

If the primary file is malformed, truncated, incompatible, or inconsistent,
loading tries the retained backup. It reports whether that backup was used and
does not rewrite either file. A store refuses to overwrite an invalid primary;
the caller must preserve or deliberately replace it outside this POC boundary.

Run the focused native check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.SaveStateTests
```

This is local model-and-file evidence. Scene save controls, reload around a
final send, player builds, and hosted worker support remain separate work.
