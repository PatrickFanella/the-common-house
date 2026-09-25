# Milestone 0 verification

Verified September 24 local time / September 25 UTC, 2026, on Kvant, in
`/home/onnwee/.t3/worktrees/the-common-house/t3code-db3a25f7`.
The work began at `b24755a` with a clean worktree. Each M0 issue has a scoped
implementation commit. Source changes are submitted for review; this report
does not claim merge, hosted CI, or automatic Open Pilot execution.

## Delivered scope

| Issue | Deliverable | Evidence |
|---|---|---|
| #1 | Portable standard-library document checker | 10 regression tests; full document scan |
| #2 | Read-only Unity readiness inspector and dated inventory | 5 regression tests; real Editor version and host inventory |
| #3 | Case 01 causal/branch contract | Six evidence records, three outcomes, recovery routes and save/repeat invariants; source comparison and link check |
| #4 | Editor-generated minimal built-in 3D project and import checker | Actual generation, configured bootstrap scene, fresh source import; 7 Python guard tests |
| #5 | Deterministic focused Unity runner and smoke fixtures | 8 Python runner checks, 2 real EditMode tests, 1 real PlayMode test, negative execution probes |

The case contract preserves explicit contact consent, visible actual recipients,
R2 as a complete outcome, and transcript-equivalent essential clues. No new
case or final canon is approved by this implementation.

## Commands and results

- `python3 -m unittest discover -s tests`: 30 passed, including an isolated T3
  run using Python 3.11.2. Tests use temporary data and require no Unity license.
- `python3 scripts/check_docs.py`: all repository README/docs local file links
  and nonempty documents passed: 18 documents and 50 local file links. This
  does not validate creative truth.
- `python3 scripts/inspect_unity.py --check-report docs/development/unity-readiness.json`:
  inventory valid; its historical read-only scope remains explicit.
- `python3 scripts/check_unity_import.py`: fresh Unity 6000.4.10f1 import passed.
- `python3 scripts/run_unity_tests.py --platform editmode --filter CommonHouse.Tests.ProjectSmokeTests`:
  2 passed, 0 failed, 0 skipped.
- `python3 scripts/run_unity_tests.py --platform playmode --filter CommonHouse.Tests.ProjectPlayModeSmokeTests`:
  1 passed, 0 failed, 0 skipped. The scene loaded, its camera was active, and a
  temporary persistent-data sentinel was written/read in the unique test namespace.
- A real nonexistent EditMode filter was rejected with exit 1 after Unity
  returned zero matching tests; it did not become a successful empty run.
- A deliberately invalid C# file in a disposable source copy made the real
  import checker fail with exit 1; the original project remained unchanged.

The runner also has regression tests for missing/malformed results, failed or
skipped tests, disagreement between result summary and individual cases,
incorrect fixture selection, missing Editor, timeout process-group termination,
nonzero Editor exit, argument handling, and cleanup.

## Evidence and cleanup

Private evidence root: `~/.local/state/common-house-unity/`. Each run retains
Editor/console logs and a result receipt. Current runners also record command
arguments, source revision/dirty paths and file hashes. Their successful
receipts identify disposable save paths. Do not post raw logs; normal native
licensing may put local account diagnostics in them.

Initial successful smoke runs:

- EditMode: `tests-editmode-20260925T012753Z-bu8wwi42`; the final focused
  issue command also passed in `tests-editmode-20260925T013212Z-00s0gmw3`.
- PlayMode: `tests-playmode-20260925T012830Z-o1cw1p0n`.
- Zero-match rejection: `tests-editmode-20260925T012924Z-fkfb863i`.
- Compiler-error rejection: `import-20260925T013024Z-rf_fmjwm`.

Temporary verification projects and their unique save directories are removed
after success or failure. Logs/XML remain. Initial project-generation scratch
state is retained separately as provenance, not mounted into verification runs.
The original source and ordinary user save identity are not used for test writes.

## Limits

Normal native licensing on Kvant worked. Redirecting Unity's configuration
home caused exit 198 in two preserved attempts. The solution preserves normal
licensed context and isolates source/save state; it does not copy or activate
a license. This is not a container security or network sandbox. The T3
container and Open Pilot worker are not qualified Unity execution environments.

Both smoke suites used headless `-nographics` execution. They do not establish
GPU rendering, graphical layout, a desktop player build, investigation gameplay,
a complete case route, or human playtest outcomes. The standard T3 recipe
remains the design-source preview/document check; Python regression commands
and native Unity commands are documented separately.

The Editor-generated pin and metadata are authoritative. Normal documentation
links and reference material: [Unity Editor command-line reference](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/editor).
Test invocation details were also checked against the installed Test Framework
1.6.0 source, especially CommandLineTest/SettingsBuilder.cs.
