# Development environment

The repository includes an Editor-generated Unity `6000.4.10f1` project,
primitive outpost with first-person movement, and Case 01 domain model alongside
the design pack. It has no database or web game server. Inspection, radio review, an evidence notebook, and a dialogue presenter are
available. The complete case scene loop is not yet wired. The existing T3 profile remains
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

## Focused Unity tests

```sh
python3 -m unittest discover -s tests -p test_unity_runner.py
python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.ProjectSmokeTests
python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ProjectPlayModeSmokeTests
```

The runner uses the pinned Editor and accepts a fully qualified namespace,
fixture, or test name. `--timeout SECONDS` bounds execution (default 240).
Both smoke suites passed with `-nographics` on Kvant. `--graphics` instead uses
the normal local display for future tests that require rendering; that mode
and wider graphical interaction are not qualified by these headless checks.

Unity Test Framework 1.6.0 is supplied by the installed Editor and recorded in
the manifest/lock. Missing tooling, version mismatch, license failure, process
failure, compiler errors, missing or malformed XML, zero selected tests, skipped
tests, failed tests, and inconsistent result counts fail the command. Each test
must belong to the requested fixture/namespace. The runner never passes `-quit`
to a test invocation, allowing Test Framework to finish writing its results.

The smoke tests check actual scene import/load and the disposable persistent-data
namespace. They intentionally require the runner's explicit test context rather
than writing to the ordinary project save location. Native execution retains
the licensing limitations described above. The successful receipts and failed
probes are summarized in [M0 verification](verification/milestone-0.md).

Run all Python regression checks through the existing isolated T3 environment:

```sh
rtk proxy ~/.local/bin/t3-dev --profile the-common-house test --command 'python3 -m unittest discover -s tests && python3 scripts/check_docs.py'
```

The unchanged default T3 Test/Check Code actions still check the document pack;
the command above adds the Python tools' regression suite. Unity tests must
use the separately qualified native runner.

Recipe ownership: `/home/onnwee/Work/Tools/t3-dev-environments` on Kvant.
Installed copy: `~/.local/share/t3-dev-environments`. The profile works with
the original design-only checkout because it needs no new repository scripts.
The private canonical remote is
`https://git.subcult.tv/PatrickFanella/the-common-house.git`.
The [implementation plan](implementation-plan.md) links the Gitea task backlog.

## Case content

The [Case 01 content guide](development/case-content.md) describes the authored
evidence asset, validation rules, and focused `CaseContentTests` command.

The [in-memory state guide](development/case-state.md) describes event retry
behavior, partial evidence, disclosure records, and `CaseStateTests`.

The [consent and routing guide](development/routing-rules.md) describes explicit
authorization, send summaries, selected material, and `RoutingRulesTests`.

The [Case 01 rehearsal](planning/case-01-rehearsal.md) includes observed text
traces for all three outcomes and the focused `CaseRehearsalTests` command.

## Outpost and local POC foundations

The [blockout verification](verification/outpost-foundations.md) covers the
three-room scene, Editor-rendered captures, and physical traversal. Open
`Assets/CommonHouse/Scenes/OutpostBlockout.unity` in the pinned Editor to inspect
the scene. [Movement controls](development/player-movement.md) use the existing
legacy input system. T3 still provides document preview; it does not launch the
game or the Editor.

The [save-store guide](development/case-persistence.md) describes versioned local
storage and disposable tests. The [observation worksheet](playtests/poc-observation-sheet.md)
is blank preparation for future owner-run sessions, not playtest evidence.

## Investigation readers

[Inspection](development/inspection.md) uses E to read and Enter to explicitly
record an inspectable item. The generated scene includes a generic shift notice.
[Radio review](development/transmission-log.md) uses R, arrow keys, and Enter;
[the notebook](development/notebook.md) uses N and arrow keys. These two readers
start empty until a case interaction acquires a source. A shared modal owner
handles Escape and movement pause, preventing panels from stealing focus.

[Dialogue presentation](development/dialogue.md) uses T, arrows, and Enter.
The generated scene currently opens an effect-free demonstration conversation;
resident-specific case interactions are separate backlog tasks.

Run `python3 scripts/capture_panels.py` to launch a temporary graphical Editor,
open the generated scene, and capture actual Game-view panels including their
text overlays. This operation needs the local display and normal Unity license.
It seeds one received call only inside the disposable copy for the reader
captures. The script exits its Editor and removes project/save state while
retaining private images and receipts. Inspect those images before making a
visual claim. This is scripted rendering evidence, not a human playtest.

[Reader verification and rendered panels](verification/investigation-readers.md)
record the local checks and their limits.

## Case interactions and source comparison

[The household opening](development/opening-household.md) offers care before
the waiting first call. Closing the welcome does not consume the call or
require a reward task. World targets share E, range checks, and occlusion with
inspection. The radio-desk call target opens Ada's transcript.

[The archive tape](development/archive-tape.md) provides E02 through a dated
transcript and sound caption. [Evidence comparison](development/evidence-comparison.md)
adds two player-selected notebook columns; unknown source times are explicit.
[Private-route inspection](development/route-inspection.md) compares the diagram
with actual recipients and records verification separately from consent.

[Case interaction verification](verification/case-interactions.md) records
the focused and integrated results and rendered panels.
