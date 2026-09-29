"""Build one character from a recipe with MPFB inside Blender and export it for Unity.

Run through scripts/build_characters.py, which pins Blender, MPFB, and the asset packs:

    blender -b --python tools/characters/mpfb_build.py -- RECIPE.json OUTPUT_DIR

The recipe names MakeHuman assets by their path below each asset type's folder, for example
"hair": "short02/short02.mhclo". The script writes OUTPUT_DIR/<id>.fbx, and writes OUTPUT_DIR/<id>.character.json with each material's MPFB
object type and texture roles (used to build Unity materials) and the asset file each texture
came from, with the copyright holders named in its asset file (used for provenance). Textures are
copied under OUTPUT_DIR/Textures/ at their path inside the asset packs.
"""
import json
import os
import shutil
import sys

import bpy

MPFB = "bl_ext.user_default.mpfb"
HumanService = __import__(MPFB + ".services.humanservice", fromlist=["HumanService"]).HumanService
ExportService = __import__(MPFB + ".services.exportservice", fromlist=["ExportService"]).ExportService
AssetService = __import__(MPFB + ".services.assetservice", fromlist=["AssetService"]).AssetService
LocationService = __import__(MPFB + ".services.locationservice", fromlist=["LocationService"]).LocationService
ObjectService = __import__(MPFB + ".services.objectservice", fromlist=["ObjectService"]).ObjectService

BODYPARTS = ("eyes", "eyebrows", "eyelashes", "teeth", "tongue", "hair")
PHENOTYPE_KEYS = ("gender", "age", "muscle", "weight", "proportions", "height", "cupsize", "firmness")


def fail(message):
    print("CHARACTER BUILD FAILED: " + message, file=sys.stderr)
    sys.exit(1)


def resolve(reference, subdir):
    path = AssetService.find_asset_absolute_path(reference, asset_subdir=subdir)
    if not path or not os.path.isfile(path):
        fail(f"cannot find {subdir} asset '{reference}'; check the recipe and installed asset packs")
    return path


def human_info(recipe):
    info = HumanService._create_default_human_info_dict()  # pylint: disable=protected-access
    phenotype = recipe.get("phenotype", {})
    for key, value in phenotype.items():
        if key == "race":
            info["phenotype"]["race"] = dict(value)
        elif key in PHENOTYPE_KEYS:
            info["phenotype"][key] = float(value)
        else:
            fail(f"unknown phenotype key '{key}'")
    for part in BODYPARTS:
        reference = recipe.get(part, "")
        if reference:
            resolve(reference, part)
        info[part] = reference
    info["clothes"] = list(recipe.get("clothes", []))
    for reference in info["clothes"]:
        resolve(reference, "clothes")
    info["skin_mhmat"] = recipe["skin"]
    resolve(recipe["skin"], "skins")
    # Simple image-texture materials, so the FBX exporter can find and copy every texture.
    info["skin_material_type"] = "MAKESKIN"
    info["eyes_material_type"] = "MAKESKIN"
    info["clothes_material_type"] = "MAKESKIN"
    info["alternative_materials"] = {}
    info["rig"] = recipe.get("rig", "game_engine")
    return info


def asset_authors(texture):
    """Copyright holders named in the header of the MakeHuman asset file beside a texture."""
    folder = os.path.dirname(texture)
    authors = []
    for name in sorted(os.listdir(folder)):
        if not name.endswith((".mhclo", ".mhmat")):
            continue
        with open(os.path.join(folder, name), encoding="utf-8", errors="replace") as handle:
            for line in handle:
                if not line.startswith("#"):
                    break
                text = line.lstrip("# ").strip()
                if text.lower().startswith("copyright (c)"):
                    holder = text[len("copyright (c)"):].strip()
                    holder = holder.split(",")[0].strip()
                    holder = " ".join(word for word in holder.split() if not word.isdigit())
                    if holder and holder not in authors:
                        authors.append(holder)
    if not authors:
        fail(f"no copyright holders found beside {texture}")
    return authors


def texture_role(node, path):
    """'normal' when the image feeds a Normal Map node, 'ao' by name, otherwise 'diffuse'."""
    for link in node.outputs[0].links:
        if link.to_node.type == "NORMAL_MAP":
            return "normal"
    name = os.path.basename(path).lower()
    if "_ao" in name:
        return "ao"
    if "_normal" in name:
        return "normal"
    return "diffuse"


def material_table(rig):
    """Per material: MPFB object type and texture paths by role, for building Unity materials."""
    table = {}
    for obj in rig.children_recursive:
        kind = (ObjectService.get_object_type(obj) or "Unknown").lower()
        for slot in getattr(obj, "material_slots", []):
            material = slot.material
            if material is None or material.name in table:
                continue
            maps = {}
            if material.use_nodes:
                for node in material.node_tree.nodes:
                    if node.type == "TEX_IMAGE" and node.image and node.image.filepath:
                        path = os.path.realpath(bpy.path.abspath(node.image.filepath))
                        maps[texture_role(node, path)] = path
            table[material.name] = {"kind": kind, "maps": maps}
    return table


def main():
    args = sys.argv[sys.argv.index("--") + 1:]
    if len(args) != 2:
        fail("expected RECIPE.json OUTPUT_DIR")
    recipe_path, output = args
    with open(recipe_path, encoding="utf-8") as handle:
        recipe = json.load(handle)
    character = recipe["id"]
    recipe.pop("note", None)
    os.makedirs(output, exist_ok=True)

    # Start from an empty scene. Factory settings would also unload the MPFB extension.
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    settings = HumanService.get_default_deserialization_settings()
    settings["subdiv_levels"] = 0
    settings["material_instances"] = "NEVER"
    basemesh = HumanService.deserialize_from_dict(human_info(recipe), settings)
    if basemesh is None:
        fail("MPFB did not create a human")

    # The copy's root is the rig; its body mesh is one of the children.
    rig = ExportService.create_character_copy(basemesh)
    if rig is None or rig.type != "ARMATURE":
        fail("the exported character has no armature")
    body = ObjectService.find_object_of_type_amongst_nearest_relatives(rig, "Basemesh")
    if body is None:
        fail("cannot find the copied body mesh")
    # Remove helper geometry and the body faces hidden under clothes, which would otherwise
    # poke through in a game engine.
    ExportService.bake_modifiers_remove_helpers(body, bake_masks=True, bake_subdiv=False, remove_helpers=True, also_proxy=True)
    bpy.ops.object.select_all(action="DESELECT")
    rig.select_set(True)
    for child in rig.children_recursive:
        if child.type == "MESH":
            child.select_set(True)
    rig.name = character
    bpy.context.view_layer.objects.active = rig

    fbx = os.path.join(output, character + ".fbx")
    # Textures are copied below rather than by the exporter, which only copies colour maps.
    bpy.ops.export_scene.fbx(
        filepath=fbx, use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, use_armature_deform_only=True,
        bake_anim=False, path_mode="STRIP", embed_textures=False)

    user_data = os.path.realpath(LocationService.get_user_data())
    textures_dir = os.path.join(output, "Textures")
    os.makedirs(textures_dir, exist_ok=True)
    sources = {}
    materials = {}
    for material, entry in sorted(material_table(rig).items()):
        maps = {}
        for role, path in entry["maps"].items():
            if not os.path.isfile(path):
                fail(f"texture {path} does not exist")
            if not path.startswith(user_data + os.sep):
                fail(f"texture {path} is outside the installed MakeHuman asset packs")
            # Keep the asset-pack layout, so characters share one copy of each texture.
            relative = os.path.relpath(path, user_data).replace(os.sep, "/")
            if relative not in sources:
                target = os.path.join(textures_dir, relative)
                os.makedirs(os.path.dirname(target), exist_ok=True)
                shutil.copyfile(path, target)
                sources[relative] = asset_authors(path)
            maps[role] = relative
        materials[material] = {"name": material, "kind": entry["kind"], **maps}
    # Lists rather than maps, so Unity's JsonUtility can read the file.
    description = {
        "id": character,
        "recipe": os.path.basename(recipe_path),
        "materials": [materials[name] for name in sorted(materials)],
        "textures": [{"path": path, "authors": sources[path]} for path in sorted(sources)],
    }
    with open(os.path.join(output, character + ".character.json"), "w", encoding="utf-8") as handle:
        json.dump(description, handle, indent=2, sort_keys=True)
        handle.write("\n")
    textures = sources
    print(f"CHARACTER BUILT: {character} -> {fbx} ({len(textures)} textures)")


if __name__ == "__main__":
    main()
