# Arrival intro authored content

Issue [#92](https://git.subcult.tv/PatrickFanella/the-common-house/issues/92) implements IN-1 from the [intro handoff](../planning/intro-agent-handoff.md).

`Assets/CommonHouse/Content/Resources/Intro.json` contains the eight arrival scenes, 66 dialogue lines, ten sources, eight question menus, and nine labelled speakers. [The script](../design/episodes/intro/script.md) owns the prose; the [scene contract](../design/episodes/intro/case-contract.md) owns intended learning and later state behavior.

`CommonHouse.Content.IntroContent.Load()` loads the Unity TextAsset separately from `CaseContent.LoadBaseline()`. `FromJson` validates version/identity, source kinds and delivery, identifiers, attribution, text/descriptions, transcript references, choices, and scene ownership. Collections and public properties are read-only. Loading creates no session, knowledge, consent, name assignment, save, transmission, or case-start effect.

Scenes carry line/source/choice references and a proposed next-scene link. These are authored data, not a running scene controller. Each option has exact text, response-line references, and a destination scene. The separate [state model](intro-state.md) implements in-memory acknowledgment and recovery. Scene presentation must preserve the contract's conditions and flexible order; the links do not implement them.

Scene descriptions are player-facing readable context. They describe the setting without showing author-only staging, identity constraints, or implementation notes. Those constraints remain in the script, contract, and handoff. IN-3 revised the descriptions for presentation without changing dialogue, choices, source coverage, or identifiers.

All sources include accessible descriptions, and spoken sources retain ordered line references whose speakers supply transcript labels. The map remains a document description. The office greeting is a recorded fragment rather than a live connection; its unknown timestamp is kept absent. The undecided company name uses the scripted generic local-service wording. Fictional live program records do not contact a real radio or internet service.

## Focused verification

```sh
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.IntroContentTests
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.CaseContentTests
```

The new fixture loads the actual Intro resource, checks baseline independence, read-only access, transcript/description coverage, optional identity choices, and invalid authoring. It verifies content only. It does not prove intro player-state transitions, directed exchanges, scene geometry, cinematics, save/reload, desktop build behavior, or human understanding.

The existing native runner uses a disposable project and separate save identity with the normal local license. See the [development guide](../development.md). Results are recorded separately in the [intro content verification report](../verification/intro-content.md).
