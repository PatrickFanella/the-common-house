# The Common House

A first-person narrative investigation game concept about found family at a communal radio outpost in an alternate future after societal collapse.

The player is a newcomer. Strange calls lead to authored cases involving the people who have offered them a home. The tone favors suspense, mystery, and creepiness. Present-day play remains at the outpost; investigable memories and flashbacks may reveal other places.

## Current state

A primitive three-room outpost with first-person movement, plus a deterministic Case 01 model and local versioned saves. All three narrative outcomes have automated text-only rehearsals. Inspection, transmission review, source comparison, resident conversations, consequential send confirmation, and matching aftermath are wired into the scene. Runtime save/reload, keyboard action paths, and shared text scaling are implemented. The full route matrix and desktop player qualification remain in progress; no human playtest is claimed. Release structure is deferred until after the POC.

## Design documents

Start with the [design index](docs/design/README.md), then read:

- [Concept brief](docs/design/concept-brief.md)
- [Game design document](docs/design/game-design-document.md)
- [Proof of concept plan](docs/design/proof-of-concept-plan.md)
- [First case](docs/design/case-01-the-road-is-clear.md)
- [Decisions and open questions](docs/design/decisions-and-open-questions.md)

`docs/design/` is the authoritative design source. The vault originals are retained as migration snapshots. [Import provenance](docs/import-provenance.json) records their source and hashes.

## Unity setup boundary

Unity `6000.4.10f1` is verified on Kvant. The Editor-generated
`ProjectSettings/ProjectVersion.txt` records its revision; `Packages/` records
the resolved dependencies. See the [readiness inventory](docs/development/unity-readiness.md)
and [project creation evidence](docs/development/unity-project.md).

Run `python3 scripts/check_unity_import.py` for a fresh import in a disposable
copy. Focused EditMode and PlayMode commands and their [verification evidence](docs/verification/milestone-0.md)
are documented in the development guide. The bootstrap uses the built-in renderer, legacy input defaults, and
explicit development company/product names. It contains no third-party assets.

Unity-generated caches, local editor state, builds, and logs are ignored. Before adding large binary assets, choose an appropriate versioning policy. No Git LFS configuration or distribution license has been selected. Open source is a game theme, not an agreed release license.

## First proof of concept

The proposed baseline is one case across a small radio room, kitchen, and archive/workshop: receive a call, inspect evidence, question a resident, route a response, and encounter a household consequence. Temporary assets are sufficient. A short investigative memory is a later POC experiment after the core loop works.

The detailed case, character names, room count, and playtest thresholds remain proposals. Confirmed creative choices are recorded separately in the decision register.

## T3 development environment

The `the-common-house` recipe provides an isolated design-document source
preview and document checks on Kvant. It does not start Unity or a playable
game. See [Development environment](docs/development.md) for commands, scope,
and the Unity setup requirements.

## Implementation tracking

The private [Gitea repository](https://git.subcult.tv/PatrickFanella/the-common-house)
owns the source and [issue backlog](https://git.subcult.tv/PatrickFanella/the-common-house/issues).
The [implementation plan](docs/implementation-plan.md) sequences small agent tasks
using the Open Pilot issue format. Case 01 is the working POC baseline; names
and story details remain provisional. Issues are not automatically queued.
