---
title: Roster and Repair-Note Evidence
type: documentation
status: draft
created: 2026-09-25
visibility: private
tags:
- project/the-common-house
- unity
---

# Roster and Repair-Note Evidence

The old Mara roster (`D01`) and Ren's current repair note (`D02`) are readable
document targets in the outpost. Open either target with `E`, read its original
text, then press `Enter` to record that document in the shared `CaseSession`.
Opening or rereading a document does not record evidence. Repeating `Enter`
reports that the document is already recorded and does not duplicate state.

`CaseInspectionBinding` connects the outpost's `InspectionController` to a
case-aware ledger after the scene starts. That ledger accepts only authored
document source IDs, rejects unknown IDs and transmissions, and uses a stable
inspection event ID for an explicit acquisition. The generic local inspection
ledger is not a fallback for Case 01 documents.

After both documents are acquired, their original texts are available in the
notebook and can be selected for side-by-side comparison. Their common repair
mark completes E03, whose authored status remains a possible identity link.
Reading, recording, or comparing those documents does not confirm identity or
authorize contact; confirmation remains E04 and authorization remains a
separate, explicit conversation decision.

Run the focused local check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.RosterEvidenceTests
```

The test covers explicit acquisition, canonical source text, notebook and E03
availability, idempotence, rejected non-document IDs, and unchanged identity
and authorization state.
