# The retro look

The outpost is drawn in the look chosen in [D-24](../design/decisions-and-open-questions.md):
models and graphics after late-1990s and early-2000s console games (GTA 3, Silent Hill,
GoldenEye 007, Mission: Impossible 64, Resident Evil, Mizzurna Falls, System Shock). The base is
PS1-style surfaces, with a PS2-era treatment over them. Gameplay, the first-person camera, and tone
are unchanged. The experiments that led here are on the `experiment/*` branches and in
`docs/experiments/` there.

## Regenerating the outpost

```sh
python3 scripts/characters.py setup   # once: pinned Blender, used for decimation
python3 scripts/build_outpost.py      # add --redecimate after changing a decimation target
```

`build_outpost.py` decimates each prop in `tools/look/decimate_targets.json` with Blender into
`Assets/CommonHouse/Art/Retro/Models/`. It then runs `OutpostSceneGenerator.Generate` in a
disposable copy, copies the result back, strips Unity's trailing whitespace, and records
`generated` provenance for the models and textures. Existing decimated models are kept, because
FBX export embeds a timestamp. Rebuilding unchanged models would only add churn.

The generator first builds the realistic outpost (CC0 materials, props, lights). Then
`CommonHouse.Editor.RetroLook` converts it:

| Part | What it does |
|---|---|
| Surfaces | Every material becomes a `CommonHouse/Retro` material. The shader lights and fogs each vertex, snaps vertices to the frame's pixel grid, maps textures affinely (without perspective correction), and samples them without filtering. |
| Textures | Each colour map is area-averaged to 64 px (`Art/Retro/Textures/`). Walls, floors, and ceilings repeat every 1.6 m. |
| Props | Each Poly Haven prop is swapped for its decimated model, lined up with the original's bounds. The chalkboard and pendant lamp keep their original meshes, because decimation broke them. |
| Lighting | `RetroLighting` passes the scene's point lights, ambient colour, and fog (5–22 m, brownish grey) to the shader. Shadows are off. |
| Frame | `RetroScreen` on the player camera renders 448 lines through HDRP dynamic resolution, scaled up with Catmull-Rom. Then `HazeFilter` (glow, warm tint, lifted blacks) and `ColorDepth` (6 bits per channel, light ordered dither) run. |

IMGUI panels are drawn after the scale-up, at full resolution, so text stays sharp.

## Things to know

- HDRP only applies dynamic resolution in play mode. Editor captures (`capture_outpost.py`)
  reproduce it by rendering 448 lines and scaling up. Only the built player or play mode shows the
  real path.
- `ColorDepth` and `HazeFilter` find their shaders by name, so the shaders live in
  `Look/Resources/`. Moving them elsewhere strips them from builds, and the player logs
  "Missing shader".
- The retro shader is unlit as far as HDRP knows. HDRP lights, shadows, decals, fog, and ambient
  occlusion do not affect it. Lamps still matter, because `RetroLighting` reads them.
- Tuning is in `RetroLighting` (ambient, fog, lamp strength), in the volume profile that
  `RetroLook` writes (haze and dither), and in `RetroScreen.lines`.
