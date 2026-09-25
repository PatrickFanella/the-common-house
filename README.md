# The Common House

A first-person narrative investigation game concept about found family at a communal radio outpost in an alternate future after societal collapse.

The player is a newcomer. Strange calls lead to authored cases involving the people who have offered them a home. The tone favors suspense, mystery, and creepiness. Present-day play remains at the outpost; investigable memories and flashbacks may reveal other places.

## Current state

Design repository, before Unity project creation. Development is planned for one person using Unity on their desktop. No playable build, scene, or game code exists yet. Release structure is deferred until after a proof of concept.

## Design documents

Start with the [design index](docs/design/README.md), then read:

- [Concept brief](docs/design/concept-brief.md)
- [Game design document](docs/design/game-design-document.md)
- [Proof of concept plan](docs/design/proof-of-concept-plan.md)
- [First case](docs/design/case-01-the-road-is-clear.md)
- [Decisions and open questions](docs/design/decisions-and-open-questions.md)

`docs/design/` is the authoritative design source. The vault originals are retained as migration snapshots. [Import provenance](docs/import-provenance.json) records their source and hashes.

## Unity setup boundary

Unity is selected, but no Editor version has been verified for this project. Unity Hub was found during repository setup; an Editor was not found in the standard local installation locations checked. This is not proof that no Editor exists elsewhere.

Once the Editor is located or installed, create the Unity project at this repository root, preserving the existing files. Commit the actual generated `Assets/`, `Packages/`, and `ProjectSettings/` contents and asset `.meta` files. Record the real Editor version through Unity's generated `ProjectVersion.txt`. Do not copy another game's settings or fabricate a version pin.

Unity-generated caches, local editor state, builds, and logs are ignored. Before adding large binary assets, choose an appropriate versioning policy. No Git LFS configuration or distribution license has been selected. Open source is a game theme, not an agreed release license.

## First proof of concept

The proposed baseline is one case across a small radio room, kitchen, and archive/workshop: receive a call, inspect evidence, question a resident, route a response, and encounter a household consequence. Temporary assets are sufficient. A short investigative memory is a later POC experiment after the core loop works.

The detailed case, character names, room count, and playtest thresholds remain proposals. Confirmed creative choices are recorded separately in the decision register.
