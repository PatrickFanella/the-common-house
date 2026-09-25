# Unity readiness inventory

Observed on Kvant at 2026-09-25 01:13 UTC (September 24 local time).
The [machine-readable report](unity-readiness.json) records the actual commands
and bounded search paths. This inventory supersedes the earlier standard-path
search that did not find an Editor.

- Editor: `/home/onnwee/Videos/6000.4.10f1/Editor/Unity`, version `6000.4.10f1`,
  verified with exit-zero `-version` output.
- Host: Omarchy 4.0.4, x86_64, kernel 7.2.5-4-omarchy.
- Memory: 64,635,879,424 bytes reported by `/proc/meminfo`.
- GPU inventory: AMD Navi 31 discrete graphics and Granite Ridge integrated
  Radeon graphics. PCI identification does not determine which GPU Unity uses
  or prove rendering performance.
- Hub: `/usr/bin/unityhub`.
- Build-support directories: Linux standalone, Android, WebGL, and iOS. Their
  presence does not establish a working build or host support for each target.

License entitlement, project import, rendering, and test execution remain
unverified by this read-only inspection. The next step may attempt normal
project creation with the installed Editor. If it reports a licensing failure,
preserve that evidence and resolve it through the normal owner login flow.
This inspection did not read license files, tokens, or Hub account state.

Reproduce or validate the inventory:

```sh
python3 scripts/inspect_unity.py --editor /home/onnwee/Videos/6000.4.10f1/Editor/Unity --output docs/development/unity-readiness.json
python3 scripts/inspect_unity.py --check-report docs/development/unity-readiness.json
python3 -m unittest discover -s tests -p test_inspect_unity.py
```

Refreshing the JSON is a new observation; update this summary with it. A valid
blocked report passes schema validation without becoming an Editor-readiness
pass. The script probes standard Linux locations and an explicit supplied
path; it does not search every filesystem or install software.
