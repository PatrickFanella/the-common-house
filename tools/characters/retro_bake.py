"""Build a low-poly retro character from a recipe and bake its look onto one small texture.

Run through `scripts/characters.py retro`:

    blender -b --python tools/characters/retro_bake.py -- RECIPE.json OUTPUT_DIR TEXTURE_SIZE

Builds the detailed MPFB character, fits MPFB's low-poly `proxy741` body (730 faces), and bakes
the detailed character's colour (skin, face, clothes, hair) onto the proxy's UVs with Cycles, the
way late-1990s console characters carried everything in one painted texture (decision D-24).
Writes OUTPUT_DIR/<id>.fbx (game-engine rig and the proxy), OUTPUT_DIR/<id>_albedo.png, and
OUTPUT_DIR/<id>.retro.json listing the source textures for provenance.
"""
import json
import os
import sys

import bpy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import mpfb_build  # noqa: E402  (shares recipe handling with the detailed build)

PROXY = "proxy741/proxy741.proxy"
BAKE_SIZE = 1024


def fail(message):
    print("RETRO BAKE FAILED: " + message, file=sys.stderr)
    sys.exit(1)


def main():
    args = sys.argv[sys.argv.index("--") + 1:]
    if len(args) != 3:
        fail("expected RECIPE.json OUTPUT_DIR TEXTURE_SIZE")
    recipe_path, output, size = args[0], args[1], int(args[2])
    with open(recipe_path, encoding="utf-8") as handle:
        recipe = json.load(handle)
    recipe.pop("note", None)
    character = recipe["id"]
    os.makedirs(output, exist_ok=True)

    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    info = mpfb_build.human_info(recipe)
    info["proxy"] = PROXY
    settings = mpfb_build.HumanService.get_default_deserialization_settings()
    settings["subdiv_levels"] = 0
    settings["material_instances"] = "NEVER"
    basemesh = mpfb_build.HumanService.deserialize_from_dict(info, settings)
    rig = basemesh.parent if basemesh.parent and basemesh.parent.type == "ARMATURE" else None
    if rig is None:
        fail("no armature")
    kinds = {obj: (mpfb_build.ObjectService.get_object_type(obj) or "").lower() for obj in rig.children_recursive if obj.type == "MESH"}
    proxies = [obj for obj, kind in kinds.items() if kind == "proxymeshes"]
    if len(proxies) != 1:
        fail(f"expected one proxy mesh, found {len(proxies)}")
    proxy = proxies[0]
    detailed = [obj for obj in kinds if obj is not proxy]
    # In this style clothes are painted onto a whole body, so keep the proxy's faces under
    # clothes (MPFB masks them out for layered clothing).
    for modifier in list(proxy.modifiers):
        if modifier.type == "MASK":
            proxy.modifiers.remove(modifier)
    # The proxy normally hides the detailed body; show it again so it can be baked from.
    for modifier in basemesh.modifiers:
        if modifier.type == "MASK" and modifier.name == "Hide base mesh":
            modifier.show_render = modifier.show_viewport = False
    # Cut-out parts (hair, brows, lashes) bake as their painted colour, without holes.
    sources = set()
    for obj in detailed:
        for slot in obj.material_slots:
            if slot.material and slot.material.use_nodes:
                for node in slot.material.node_tree.nodes:
                    if node.type == "TEX_IMAGE" and node.image and node.image.filepath:
                        sources.add(os.path.relpath(os.path.realpath(bpy.path.abspath(node.image.filepath)),
                                                    os.path.realpath(mpfb_build.LocationService.get_user_data())).replace(os.sep, "/"))

    image = bpy.data.images.new(character + "_albedo", BAKE_SIZE, BAKE_SIZE, alpha=False)
    material = bpy.data.materials.new(character + "_retro")
    material.use_nodes = True
    node = material.node_tree.nodes.new("ShaderNodeTexImage")
    node.image = image
    material.node_tree.nodes.active = node
    proxy.data.materials.clear()
    proxy.data.materials.append(material)

    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.cycles.samples = 4
    bake = scene.render.bake
    bake.use_selected_to_active = True
    bake.cage_extrusion = 0.03
    bake.max_ray_distance = 0.12
    bake.margin = 4
    bpy.ops.object.select_all(action="DESELECT")
    for obj in detailed:
        obj.hide_render = False
        obj.select_set(True)
    proxy.select_set(True)
    bpy.context.view_layer.objects.active = proxy
    bpy.ops.object.bake(type="DIFFUSE", pass_filter={"COLOR"}, use_selected_to_active=True,
                        cage_extrusion=0.03, max_ray_distance=0.12, margin=4)
    # Baked large for clean edges, then shrunk to console size.
    image.scale(size, size)
    albedo = os.path.join(output, character + "_albedo.png")
    image.filepath_raw = albedo
    image.file_format = "PNG"
    image.save()

    for obj in detailed:
        bpy.data.objects.remove(obj, do_unlink=True)
    rig.name = character
    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    proxy.select_set(True)
    bpy.context.view_layer.objects.active = rig
    fbx = os.path.join(output, character + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=fbx, use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, use_armature_deform_only=True,
        bake_anim=False, path_mode="STRIP", embed_textures=False)
    faces = len(proxy.data.polygons)
    with open(os.path.join(output, character + ".retro.json"), "w", encoding="utf-8") as handle:
        json.dump({"id": character, "recipe": os.path.basename(recipe_path), "faces": faces,
                   "texture_size": size, "sources": sorted(sources)}, handle, indent=2, sort_keys=True)
        handle.write("\n")
    print(f"RETRO CHARACTER BUILT: {character} -> {fbx} ({faces} faces, {size} px texture)")


main()
