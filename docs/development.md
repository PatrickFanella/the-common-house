# Development environment

This repository is a design pack. There is no Unity project, Editor version
pin, package manifest, database, game server, or gameplay test suite yet.
The T3 profile `the-common-house` supports this stage with Python 3 from the
shared `t3-dev-tools:20260924-v3` image. It requires no package installation.
Setup checks the design entry point and parses the import provenance JSON.

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

## Next stage

Locate or install an appropriate Unity Editor through the normal desktop
workflow. Create the actual Unity project at the repository root while
preserving the design pack, then record the generated Editor version and
package files. Update the canonical recipe after qualifying real Editor,
build, and test commands. This setup does not establish Unity licensing, GPU
access, a build target, or headless Unity support.

Recipe ownership: `/home/onnwee/Work/Tools/t3-dev-environments` on Kvant.
Installed copy: `~/.local/share/t3-dev-environments`. The profile works with
the original design-only checkout because it needs no new repository scripts.
The private canonical remote is
`https://git.subcult.tv/PatrickFanella/the-common-house.git`.
The [implementation plan](implementation-plan.md) links the Gitea task backlog.
