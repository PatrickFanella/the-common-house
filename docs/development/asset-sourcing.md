# Asset sourcing and provenance

Decisions [D-21 and D-22](../design/decisions-and-open-questions.md) allow CC0 scanned
materials alongside procedural generation, and store textures and models with Git LFS. This
page describes how to add a sourced file.

## Allowed sources

| Source | Asset pages | License |
|---|---|---|
| Poly Haven | `https://polyhaven.com/a/<asset>` | CC0-1.0 |
| ambientCG | `https://ambientcg.com/view?id=<asset>` | CC0-1.0 |

Check the license on the asset page when downloading. Adding another source or license needs
a new decision; the checker rejects anything else.

## Adding a file

1. Run `git lfs install` once in the clone, so textures and models are committed as LFS
   pointers.
2. Put the files under `Assets/CommonHouse/Art/Sourced/<Source>/<asset>/`, keeping the
   source's file names. Let Unity create the `.meta` files.
3. Add one entry per file (not per `.meta`) to
   [`asset-provenance.json`](asset-provenance.json):

   ```json
   {
     "path": "Assets/CommonHouse/Art/Sourced/PolyHaven/wood_floor/wood_floor_diff_2k.png",
     "kind": "cc0",
     "source": "Poly Haven",
     "url": "https://polyhaven.com/a/wood_floor",
     "authors": ["Name as shown on the asset page"],
     "license": "CC0-1.0",
     "retrieved": "2026-09-26",
     "sha256": "<sha256sum of the file as committed>",
     "changes": "Optional: resized from 4k to 2k"
   }
   ```

   If a page names no individual author, use the source's name. Record any edit made
   before committing in `changes`, and hash the edited file.
4. Run the checks:

   ```sh
   python3 scripts/check_provenance.py
   python3 scripts/check_binary_sizes.py
   ```

Textures that project code bakes into files use `"kind": "generated"` and a `generator`
path to the code that produced them, instead of the source fields. Procedural materials
built in memory, such as the style board's, are not files and need no entry.

## What the checks enforce

`check_provenance.py` requires an entry for every tracked file under
`Assets/CommonHouse/Art/Sourced/` except `.meta` files, and for every LFS file anywhere.
It fails when an entry has an unknown source or kind, a non-CC0 license, a URL on another
host, an empty author list, a malformed date, unknown fields, or a hash that does not match.
It also fails on a duplicate entry or one that names an untracked file. For an LFS file that
has not been fetched, it compares against the pointer's oid, which is the same SHA-256.

`check_binary_sizes.py` skips LFS files when applying the 5 MiB cap. It fails when a file
matches an LFS pattern but its staged content is not an LFS pointer, which happens when
`git lfs install` was not run. LFS patterns match lowercase extensions only; a file with an
uppercase extension stays in plain Git under the cap.

The checks confirm that the record is complete and consistent. They do not confirm the
license on the source site; the person adding the file does that.
