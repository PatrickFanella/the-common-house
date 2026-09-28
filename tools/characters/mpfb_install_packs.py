"""Install MakeHuman asset pack zips into MPFB's user data folder. Run inside Blender:

    blender -b --python tools/characters/mpfb_install_packs.py -- PACK.zip [PACK.zip ...]
"""
import sys

MPFB = "bl_ext.user_default.mpfb"
AssetService = __import__(MPFB + ".services.assetservice", fromlist=["AssetService"]).AssetService
LocationService = __import__(MPFB + ".services.locationservice", fromlist=["LocationService"]).LocationService

target = LocationService.get_user_data()
for pack in sys.argv[sys.argv.index("--") + 1:]:
    problem = AssetService.check_asset_pack_zip(pack) or AssetService.fix_and_extract_asset_pack_zip(pack, target)
    if problem:
        print(f"PACK INSTALL FAILED: {pack}: {problem}", file=sys.stderr)
        sys.exit(1)
    print(f"PACK INSTALLED: {pack}")
AssetService.update_all_asset_lists()
