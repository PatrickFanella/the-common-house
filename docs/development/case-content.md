# Case 01 authored content

Issue [#6](https://git.subcult.tv/PatrickFanella/the-common-house/issues/6)
implements the six evidence records from the
[causal contract](../planning/case-01-contract.md).
`Assets/CommonHouse/Content/Resources/Case01.json` is a Unity TextAsset loaded
by `CommonHouse.Content.CaseContent.LoadBaseline()` through `Resources.Load`
and `JsonUtility`. It needs no new package or service.

Stable identifiers cover E01–E06, R1–R3, residents `ren` and `ivo`, caller `ada`,
transmissions T01–T03, documents D01–D03, conversations C01–C02, and recovery
paths. Names, source excerpts, and the shared repair detail remain provisional.
These are text-only rehearsal excerpts, not final dialogue or recorded audio.
No audio asset is implied by a transmission record.

Each source includes readable text, its kind, speaker attribution where spoken,
and a recovery reference. T02 includes the kitchen bell caption. Evidence
statements distinguish observation, attributed claim, hypothesis, and
corroboration, with references back to their supporting sources. E03 keeps the
roster and repair note separate; E06 keeps Ada’s and Ren’s accounts separate.
All listed sources are required to complete a composite record. Tracking a
partially acquired record belongs to issue #7.

The loader rejects missing or duplicate IDs, IDs outside the baseline cast,
evidence and outcome sets, dangling source/recovery references, absent text,
missing bell captions, unknown statement kinds, and claims attributed to
someone other than their source speaker. It exposes read-only collections and
properties. The authored model contains no acquisition, consent, draft,
resolution, or save state. Loading all content is an author/runtime operation;
a future notebook must select only what the player has actually learned.

Run the focused check from the repository root:

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseContentTests
```

The tests load the real Unity resource and mutate copies of its JSON to check
invalid authoring. They also check the six-record structure, source attribution,
composite clues, and read-only API. This verifies content loading and validation;
state transitions, consent enforcement, and route rehearsal belong to issues
#7–#9. Scene presentation, player builds, and human comprehension remain separate
verification steps. See the [development guide](../development.md) for native
Editor licensing, disposable state, and cleanup requirements.
