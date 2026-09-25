# Local Linux POC build verification

`python3 scripts/build_poc.py --target local --smoke` builds the enabled Case 01 scene with the Editor pinned in `ProjectSettings/ProjectVersion.txt`, then launches that Linux player and performs the explicit R2 smoke route. The command requires the installed matching Linux standalone support and the normal local Unity licensing context. It never installs an Editor, build support, or license.

The runner copies only `Assets/`, `Packages/`, and `ProjectSettings/` into a disposable project. It rejects project symlinks, credential-like source files, and unreviewed local or Git package references. It gives that copy a unique `CommonHouseVerification/run-*` persistent-data identity, retains the normal host licensing context, and removes both the copied project and the unique save directory afterward. The build output, source receipt, sorted output hash manifest, player screenshot, structured smoke receipt, and private logs remain below `~/.local/state/common-house-unity/local-poc-build-*`.

The target is only Linux x86_64:

```sh
python3 scripts/build_poc.py --target local --smoke
```

Use `--editor /absolute/path/to/Unity` only for another installed Editor that exactly matches the pin. `--timeout SECONDS` bounds each Editor and player process. A timeout terminates the owned process group and fails the command.

The player smoke runs only when both `COMMON_HOUSE_ENV=test` and the exact command argument `--common-house-smoke=r2` are present. Normal development and production-style launches do not activate it. The smoke uses the generated scene's focus and interaction controllers to receive the call, speak privately with Ren, and prepare R2. It verifies the automatic pre-send checkpoint reloads to the unresolved R2 draft, sends again, completes the refusal aftermath, then performs a manual post-send save/load. It requires exactly one outgoing `radio-send:R2` disclosure, captures the player aftermath view, writes `smoke-result.json`, and exits.

The command fails for a missing/mismatched Editor, unavailable build support, license or compilation failure, absent/non-executable player output, malformed or missing receipt, output-hash failure, player crash or timeout, failed R2 state assertions, missing screenshot, or a leftover disposable save directory. It records the selected target, source revision and dirty state, input source hashes, full output manifest hash, Editor build receipt, player command, and disposable save path in its private `result.json`.

This is scripted local desktop-player evidence. It proves one local Linux build and scripted scene-controller route under disposable data. It does not prove a human playtest, input-device accessibility beyond the route checks, platform distribution readiness, performance, or Open Pilot worker support.
