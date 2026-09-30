# City asset kit verification

Local asset preparation for [issue #90](https://git.subcult.tv/PatrickFanella/the-common-house/issues/90).
See the [asset guide](../development/city-assets.md) for generation and use.

Generation produced 20 prefabs and 13 original textures. The corrected focused native fixture passed all five tests, with no failures
or skips. All four corrected city captures were inspected; the complete graphical EditMode suite also passed all 99 tests, with no
failures or skips. The initial focused run passed four checks and failed one
seam check because it read collider bounds before synchronizing the moved
prefabs. Render inspection also exposed mirrored cube UVs on signs. The
fixture now synchronizes physics first; signs use an original persistent quad
mesh with explicit UVs. Initial failure evidence is retained.

The isolated T3 snapshot passed 64 Python tests and checked 78 Markdown
documents / 272 local links. Provenance and size checks passed with 289
entries/LFS files and 73 plain-Git binaries below 5 MiB.
The scope is a reusable city prefab kit and isolated review scene. It does not
claim a playable flashback, complete city, human playtest, desktop build,
hosted CI, publication, or final location/story art.

## Native receipts

Unity 6000.4.10f1 on Kvant, normal existing license and graphical Vulkan context.
Final generation: `editor-method-20260930T005336Z-162jmyh5`.
Focused graphical fixture: `tests-editmode-20260930T005545Z-xl5qvzhj`,
5 passed, 0 failed/skipped. Views inspected: street overview, storefront,
street furniture, and service alley. The sign orientation check validates
persistent face-mesh UV direction as well as the corrected rendered text.

Initial fixture: `tests-editmode-20260930T005001Z-pr5_vq4a`, 4 passed / 1 failed.
Its seam check read unsynchronized collider bounds; the final fixture performs
`Physics.SyncTransforms` before sampling bounds and casting across the seam.
The initial sign renders were mirrored and corrected before final verification.

The outpost scene and Editor build settings remain byte-identical to 7a26e7a.
The city study has no case/player/flashback controllers and is excluded from
build settings. These are asset and render checks, not playable flashback or
new Linux build evidence.

Full graphical EditMode receipt: `tests-editmode-20260930T005756Z-0xk6jd63`,
99 passed, 0 failed/skipped. All 1,348 Unity input hashes in both final test
receipts match the checkout. Disposable project/save directories were removed;
private logs, source receipts, XML and captures remain under
`~/.local/state/common-house-unity/`. No new desktop build was run: the city
study is excluded from the playable build and its integration is a later task.
