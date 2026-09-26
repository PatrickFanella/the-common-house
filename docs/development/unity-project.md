# Unity project creation and import

Verified on Kvant, September 24 local time / September 25 UTC, 2026.
The [readiness inventory](unity-readiness.md) identified the real installed
Editor before creation. This report records later execution evidence.

## Generated source

Unity `6000.4.10f1` (`feeafc12a938`) created a new empty project with
`-batchmode -nographics -quit -createProject` in a private disposable directory.
The command returned 0 and generated `ProjectVersion.txt`; no other game's
project/settings were copied. The explicit one-time
`CommonHouse.Editor.ProjectBootstrap.Configure` method then created a default
camera/light scene and configured the development company/product identity
and text serialization. It refuses to replace an existing bootstrap scene.

Only generated `Assets/`, `Packages/`, and `ProjectSettings/` were copied to
this repository root; design documents and history were preserved. Asset
`.meta` files came from the Editor. Library, user settings, and logs were not
added to Git.

The project was created with the built-in 3D renderer and the generated legacy input
configuration. It moved to HDRP on 2026-09-26; see the
[HDRP migration report](../verification/hdrp-migration.md). Multiplayer Center and the unused ARM64 SDK were removed from
the generated manifest. Linux x86_64 SDK/toolchain entries and built-in engine
modules remain; the Editor regenerated the dependency lock. No paid assets,
external dialogue framework, cloud project, or release license was introduced.

## Verified boundary

`python3 scripts/check_unity_import.py` passed against a fresh source copy,
using the exact generated version. The native Editor exited 0 with its batch
completion marker and no recognized compilation/license failure. Seven Python
checks cover source-copy boundaries, package/symlink rejection, version
mismatch, missing/failed import logs, and unique save identities.

Bootstrap evidence: `~/.local/state/common-house-unity/bootstrap-20260925T011629Z/`.
Successful configuration evidence:
`~/.local/state/common-house-unity/configure-20260925T012123Z-xrhalz6k/`.
Fresh import evidence:
`~/.local/state/common-house-unity/import-20260925T012215Z-o20tpsro/`.

Two earlier configuration attempts returned 198 when the XDG licensing context
was redirected. Their private logs remain in their own configure directories.
The successful run preserved normal licensed context; it did not activate or
copy a license. Normal local execution succeeded, but no claim follows about
a container, another host, or unattended Gitea/Open Pilot execution.

No gameplay, GPU rendering, desktop build, or human playtest has been qualified.
The scene is a startup foundation, not the outpost or a playable investigation.
