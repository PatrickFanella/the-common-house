"""Reduce a model to about a target triangle count for the N64 experiment (issue #67).

    blender -b --python tools/experiments/n64_decimate.py -- INPUT.fbx OUTPUT.fbx TRIANGLES

Imports the FBX, applies a collapse Decimate modifier with one ratio across all meshes, and
exports with the same material names, so Unity can remap them like the original model.
"""
import sys

import bpy

args = sys.argv[sys.argv.index("--") + 1:]
source, target, triangles = args[0], args[1], int(args[2])
for obj in list(bpy.data.objects):
    bpy.data.objects.remove(obj, do_unlink=True)
bpy.ops.import_scene.fbx(filepath=source)
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
if not meshes:
    print("DECIMATE FAILED: no meshes in " + source, file=sys.stderr)
    sys.exit(1)


def count(objects):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    total = 0
    for obj in objects:
        mesh = obj.evaluated_get(depsgraph).to_mesh()
        mesh.calc_loop_triangles()
        total += len(mesh.loop_triangles)
        obj.evaluated_get(depsgraph).to_mesh_clear()
    return total


before = count(meshes)
ratio = min(1.0, triangles / max(before, 1))
for obj in meshes:
    # Decimate before the armature deform, so the rest pose is what gets simplified.
    modifier = obj.modifiers.new("N64 decimate", "DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    obj.modifiers.move(len(obj.modifiers) - 1, 0)
after = count(meshes)
bpy.ops.object.select_all(action="SELECT")
# Armatures and their weights survive: collapse decimation interpolates vertex groups.
bpy.ops.export_scene.fbx(filepath=target, use_selection=True, object_types={"MESH", "EMPTY", "ARMATURE"}, use_mesh_modifiers=True,
                         add_leaf_bones=False, use_armature_deform_only=True,
                         apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
                         mesh_smooth_type="FACE", path_mode="STRIP", embed_textures=False, bake_anim=False)
print(f"DECIMATED: {source} {before} -> {after} triangles")
