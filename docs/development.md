# Development environment

The repository now includes an Editor-generated Unity `6000.4.10f1` project
and empty 3D bootstrap scene, alongside the design pack. It has no database,
web game server, or investigation gameplay yet. The existing T3 profile remains
a document-source preview and link check using Python 3; it does not launch Unity.

## Actions and commands

Portable document checks need Python 3.10 or newer and no packages:

```sh
python3 scripts/check_docs.py
python3 scripts/check_docs.py docs/design/case-01-the-road-is-clear.md
python3 -m unittest discover -s tests -p test_check_docs.py
```

The checker supports simple inline Markdown links and images, angle-bracket
destinations, percent-encoded filenames, and optional quoted titles. It skips
fenced/inline code, external URLs, and anchor validation. Reference-style links,
HTML links, escaped delimiters, and nested-parenthesis destinations are outside
its scope. Local targets must be files inside the repository, including after
symlink resolution. The existing T3 recipe remains unchanged.

The T3 project on Kvant has Setup Worktree, Start Dev, Watch Changes, Sync
Changes, Preview URL, Test, Check Code, Dev Status, Dev Logs, and Stop Dev.
New threads default to worktrees; existing threads keep their current checkout.
Run these commands from the intended checkout:

```sh
rtk proxy ~/.local/bin/t3-dev --profile the-common-house start
rtk proxy ~/.local/bin/t3-dev --profile the-common-house url
rtk proxy ~/.local/bin/t3-dev --profile the-common-house sync
rtk proxy ~/.local/bin/t3-dev --profile the-common-house test
rtk proxy ~/.local/bin/t3-dev --profile the-common-house verify
rtk proxy ~/.local/bin/t3-dev --profile the-common-house stop
```

Start Dev serves the copied `docs/` directory. Open `/design/` beneath the
printed loopback URL to browse the design files. Markdown is served as source,
not rendered HTML. Edit this worktree, then Sync Changes, or run Watch Changes
in a separate terminal. Stop the watch terminal separately when finished.

Test checks that Markdown documents are nonempty and that simple inline local
file links in the repository README and `docs/` resolve inside the snapshot.
Check Code also parses `docs/import-provenance.json`. These commands do not
check remote links, heading anchors, every Markdown link syntax, historical
import hashes, creative consistency, Unity compilation, or gameplay.

## Isolation and cleanup

The launcher copies source into worktree-specific development state. App and
test snapshots are separate. Preview uses `APP_ENV=development`; checks use
`APP_ENV=test`. No database or Redis service is configured, and their preview
connection variables are empty. No production data, credentials, or Unity
license is needed. The app network is isolated and the host preview binds to
loopback. Tests remove their containers and networks when done. Stop Dev
removes preview containers and networks while retaining snapshots and caches
under `~/.local/state/t3-dev-environments/fleet/`.

## Native Unity import

```sh
python3 scripts/check_unity_import.py
# On another machine, supply its actual matching Editor:
python3 scripts/check_unity_import.py --editor /absolute/path/to/Editor/Unity
```

`UNITY_EDITOR` is also accepted. Without an override, the tool uses the path
in the Kvant readiness inventory and verifies its version against the generated
project pin. It fails if that Editor is absent or mismatched. This is a Linux
local-desktop tool; it does not install Unity, activate licenses, or configure
a remote worker. Python 3.10 or newer is required.

Only `Assets/`, `Packages/`, and `ProjectSettings/` enter the temporary project.
External symlinks, credential-like files, and unreviewed local/Git package
references are rejected. In the copy only, a unique company/product identity
keeps Unity's persistent-data path separate from user saves. The temporary
project and that run's save directory are removed afterward. Logs, source
hashes, and result receipts remain privately under
`~/.local/state/common-house-unity/`. These may contain local licensing
diagnostics; do not commit or post the raw logs.

Unity needs the existing normal HOME/XDG licensing context. Redirecting its
configuration home caused exit 198 during qualification; no license data was
copied or activation changed. Native verification isolates project and save
state, but it is not a container security/network sandbox. Do not use it to run
unreviewed source. The T3 container and Open Pilot worker remain unqualified
for Unity. See [project evidence](development/unity-project.md).

Recipe ownership: `/home/onnwee/Work/Tools/t3-dev-environments` on Kvant.
Installed copy: `~/.local/share/t3-dev-environments`. The profile works with
the original design-only checkout because it needs no new repository scripts.
The private canonical remote is
`https://git.subcult.tv/PatrickFanella/the-common-house.git`.
The [implementation plan](implementation-plan.md) links the Gitea task backlog.
