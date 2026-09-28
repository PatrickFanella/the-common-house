using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static CommonHouse.Editor.StyleBoard.ProceduralSurfaces;

namespace CommonHouse.Editor.StyleBoard
{
    /// <summary>Proposed palette entries. Names describe use, not approved canon.</summary>
    internal static class StylePalette
    {
        public static readonly Color HoneyTimber = Hex("#9a6a3c");
        public static readonly Color DarkTimber = Hex("#4a2e1c");
        public static readonly Color GreyTimber = Hex("#8a7f70");
        public static readonly Color PlasterCream = Hex("#cbbf9f");
        public static readonly Color KitchenTeal = Hex("#4f7f7a");
        public static readonly Color EnamelCream = Hex("#d9d0b8");
        public static readonly Color EnamelRed = Hex("#9c3a28");
        public static readonly Color Mustard = Hex("#b88d36");
        public static readonly Color RadioGreen = Hex("#56614f");
        public static readonly Color UtilityBlue = Hex("#4c5a66");
        public static readonly Color FuturePolymer = Hex("#17191c");
        public static readonly Color IndicatorCyan = Hex("#58d6e6");
        public static readonly Color DialAmber = Hex("#ffa040");
        public static readonly Color MaskingTape = Hex("#d2bd85");
        public static readonly Color Copper = Hex("#b87348");
        public static readonly Color Brass = Hex("#b59a52");
        public static readonly Color Paper = Hex("#e6dcc3");
        public static readonly Color Cardboard = Hex("#a88557");
        public static readonly Color Ink = Hex("#1f2430");
        public static readonly Color RedInk = Hex("#8c2020");
        public static readonly Color PencilGrey = Hex("#4d4d4d");

        public static IEnumerable<(string Name, Color Color)> Entries()
        {
            foreach (var field in typeof(StylePalette).GetFields())
                if (field.FieldType == typeof(Color))
                    yield return (field.Name, (Color)field.GetValue(null));
        }

        public static Color Hex(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var color)) throw new ArgumentException("Bad colour " + hex);
            return color;
        }
    }

    /// <summary>
    /// The style-study material library: CC0 scans for the largest surfaces (decision D-21) and
    /// generated textures for the rest, all on HDRP/Lit.
    /// </summary>
    internal sealed class StyleMaterials
    {
        private readonly int size;
        private readonly Shader lit;
        public readonly List<(string Name, Material Material)> Library = new List<(string, Material)>();

        public Material Timber, DarkTimber, WeatheredTimber, Floorboards, Plaster, KitchenTiles, WallTiles;
        public Material RadioPaint, UtilityPaint, TealPaint, CreamEnamel, RedEnamel, MustardPaint, PegboardPaint;
        public Material Polymer, Copper, Brass, Steel, Paper, OldPaper, Cardboard, Tape, Tablecloth, Cork, Cable, RedCable, ClothTape;
        public Material Glass, Bulb, Dial, Indicator, NightWindow, Tube, Ink, RedInk, Pencil, Backdrop;

        public StyleMaterials(int textureSize)
        {
            size = textureSize;
            lit = Shader.Find("HDRP/Lit");
            if (lit == null) throw new InvalidOperationException("HDRP/Lit is unavailable; the style board needs HDRP.");
            // Scans, tiled at their real size (metres) because study meshes use metre UVs.
            Timber = Scan("Worn table timber (scan)", "wood_table_worn", 0.55f);
            Floorboards = Scan("Worn floorboards (scan)", "wood_floor_worn", 2.0f);
            Plaster = Scan("Plastered wall (scan)", "plastered_wall_04", 3.2f);
            KitchenTiles = Scan("Worn floor tiles (scan)", "worn_tile_floor", 2.0f);
            // The same table scan, darkened or greyed, for frames, legs, shelving, and salvage.
            DarkTimber = Scan("Dark timber (scan)", "wood_table_worn", 0.55f, Hex(0.42f));
            WeatheredTimber = Scan("Weathered timber (scan)", "wood_table_worn", 0.55f, new Color(0.95f, 1f, 1.05f), 0.55f);
            WallTiles = Make("Splashback tiles", 1.2f, 1.6f, (u, v) => Tiles(u, v, StylePalette.EnamelCream, Darken(StylePalette.EnamelCream, 0.08f), Hex(0.5f), 6, 67));
            RadioPaint = Make("Radio green paint", 2f, 2f, (u, v) => Painted(u, v, StylePalette.RadioGreen, Hex(0.55f), 1f, 0.7f, 71));
            UtilityPaint = Make("Utility blue paint", 2f, 2f, (u, v) => Painted(u, v, StylePalette.UtilityBlue, Hex(0.55f), 1f, 0.68f, 73));
            TealPaint = Make("Teal paint on timber", 2f, 1.5f, (u, v) => Painted(u, v, StylePalette.KitchenTeal, StylePalette.HoneyTimber, 0f, 0.64f, 79));
            CreamEnamel = Make("Cream enamel", 1.5f, 2f, (u, v) => Painted(u, v, StylePalette.EnamelCream, Hex(0.15f), 0.8f, 0.76f, 83));
            RedEnamel = Make("Red enamel", 1.5f, 3f, (u, v) => Painted(u, v, StylePalette.EnamelRed, Hex(0.15f), 0.8f, 0.74f, 89));
            MustardPaint = Make("Mustard paint", 1.5f, 3f, (u, v) => Painted(u, v, StylePalette.Mustard, Hex(0.5f), 1f, 0.7f, 97));
            PegboardPaint = Make("Pegboard", 1f, 1f, (u, v) => Pegboard(u, v, Lighten(StylePalette.UtilityBlue, 0.35f), 24, 101));
            Polymer = Make("Future polymer", 1f, 3f, (u, v) => ProceduralSurfaces.Polymer(u, v, StylePalette.FuturePolymer, 103));
            Copper = Make("Copper", 1f, 4f, (u, v) => Tarnished(u, v, StylePalette.Copper, Hex("#4f7a62"), 107));
            Brass = Make("Brass", 1f, 4f, (u, v) => Tarnished(u, v, StylePalette.Brass, Hex("#4a4230"), 109));
            Steel = Make("Steel", 1f, 3f, (u, v) => Tarnished(u, v, Hex(0.62f), Hex("#5a3a24"), 113));
            Paper = Make("Paper", 0.5f, 2f, (u, v) => ProceduralSurfaces.Paper(u, v, StylePalette.Paper, 127));
            OldPaper = Make("Old paper", 0.5f, 2f, (u, v) => ProceduralSurfaces.Paper(u, v, Darken(StylePalette.Paper, 0.12f), 131));
            Cardboard = Make("Cardboard", 0.5f, 2f, (u, v) => ProceduralSurfaces.Paper(u, v, StylePalette.Cardboard, 137));
            Tape = Make("Masking tape", 0.4f, 6f, (u, v) => ProceduralSurfaces.Paper(u, v, StylePalette.MaskingTape, 139));
            Tablecloth = Make("Tablecloth", 1.2f, 1.2f, (u, v) => Check(u, v, StylePalette.EnamelCream, StylePalette.EnamelRed, 6, 149));
            Cork = Make("Cork", 0.6f, 1f, (u, v) => ProceduralSurfaces.Cork(u, v, 151));
            Cable = Make("Black cable", 0.6f, 6f, (u, v) => Rubber(u, v, Hex(0.07f), 157));
            RedCable = Make("Red cable", 0.6f, 6f, (u, v) => Rubber(u, v, Darken(StylePalette.EnamelRed, 0.15f), 163));
            ClothTape = Make("Cloth tape", 0.8f, 8f, (u, v) => Rubber(u, v, Hex(0.12f), 167));
            Glass = Transparent("Glass", new Color(0.75f, 0.85f, 0.85f, 0.22f));
            // Emission in nits; compare with the exposure values in StyleStudies.
            Bulb = Emissive("Warm bulb", new Color(1f, 0.78f, 0.5f), 3000f);
            Dial = Emissive("Amber dial", StylePalette.DialAmber, 60f);
            Indicator = Emissive("Cyan indicator", StylePalette.IndicatorCyan, 80f);
            NightWindow = Emissive("Night window", new Color(0.1f, 0.14f, 0.24f), 25f);
            Tube = Emissive("Cool tube", new Color(0.85f, 0.93f, 1f), 2000f);
            Ink = Text("Ink", StylePalette.Ink);
            RedInk = Text("Red ink", StylePalette.RedInk);
            Pencil = Text("Pencil", StylePalette.PencilGrey);
            Backdrop = Plain("Backdrop", Hex(0.32f), 0.1f);
        }

        /// <summary>Gives the ink materials an RGBA copy of the font atlas; call after all labels exist.</summary>
        public void ApplyFontAtlas(Font font)
        {
            var atlas = FontAtlas(font);
            foreach (var ink in new[] { Ink, RedInk, Pencil }) ink.SetTexture("_BaseColorMap", atlas);
        }

        private Material Make(string name, float normalStrength, float tiling, Func<float, float, SurfaceSample> sample)
        {
            var surface = Build(name, size, normalStrength * size / 256f, sample);
            return Register(name, Textured(name, surface.Albedo, surface.Normal, surface.Mask, tiling));
        }

        /// <summary>A scanned material; tint multiplies the albedo, and desaturate greys it toward its luminance.</summary>
        private Material Scan(string name, string asset, float metres, Color? tint = null, float desaturate = 0f)
        {
            var folder = ScanTextures.PolyHaven(asset);
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "_diff_2k.jpg");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + "_nor_gl_2k.jpg");
            if (albedo == null || normal == null) throw new InvalidOperationException("Missing scan textures for " + asset);
            if (!masks.TryGetValue(asset, out var mask)) masks[asset] = mask = ScanTextures.MaskFromArm(folder + "_arm_2k.jpg");
            var material = Textured(name, albedo, normal, mask, 1f / metres);
            if (desaturate > 0f)
            {
                // HDRP/Lit has no saturation control, so bake a greyed copy of the albedo instead.
                material.SetTexture("_BaseColorMap", Desaturated(folder + "_diff_2k.jpg", desaturate));
            }
            if (tint.HasValue) material.SetColor("_BaseColor", tint.Value);
            return Register(name, material);
        }

        private readonly Dictionary<string, Texture2D> masks = new Dictionary<string, Texture2D>();

        private static Texture2D Desaturated(string path, float amount)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            if (!texture.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Cannot decode " + path);
            var pixels = texture.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var grey = (byte)Mathf.Clamp(0.2126f * p.r + 0.7152f * p.g + 0.0722f * p.b, 0f, 255f);
                pixels[i] = Color32.Lerp(p, new Color32(grey, grey, grey, 255), amount);
            }
            texture.SetPixels32(pixels);
            texture.Apply(true);
            texture.name = Path.GetFileNameWithoutExtension(path) + "-grey";
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.anisoLevel = 8;
            return texture;
        }

        private Material Register(string name, Material material)
        {
            Library.Add((name, material));
            return material;
        }

        private Material Textured(string name, Texture albedo, Texture normal, Texture mask, float tiling)
        {
            var material = new Material(lit) { name = name };
            material.SetTexture("_BaseColorMap", albedo);
            material.SetTextureScale("_BaseColorMap", new Vector2(tiling, tiling));
            material.SetTexture("_NormalMap", normal);
            material.SetFloat("_NormalScale", 1f);
            material.SetTexture("_MaskMap", mask);
            material.SetFloat("_MetallicRemapMax", 1f);
            material.SetFloat("_SmoothnessRemapMax", 1f);
            material.SetFloat("_AORemapMax", 1f);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        private Material Plain(string name, Color color, float smoothness)
        {
            var material = new Material(lit) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        private Material Emissive(string name, Color color, float nits)
        {
            var material = Plain(name, color * 0.3f, 0.6f);
            material.SetFloat("_UseEmissiveIntensity", 1f);
            HDMaterial.SetEmissiveColor(material, color);
            HDMaterial.SetEmissiveIntensity(material, nits, EmissiveIntensityUnit.Nits);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        private Material Transparent(string name, Color color)
        {
            var material = Plain(name, color, 0.95f);
            HDMaterial.SetSurfaceType(material, true);
            return material;
        }

        private Material Text(string name, Color ink)
        {
            // Lit and alpha-clipped, so handwriting takes the scene's light instead of glowing.
            var material = Plain(name, ink, 0.2f);
            HDMaterial.SetAlphaClipping(material, true);
            HDMaterial.SetAlphaCutoff(material, 0.5f);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        /// <summary>
        /// HDRP/Lit multiplies base colour by the texture's RGB, and the dynamic font atlas stores
        /// glyph coverage in a single channel, so copy it into white RGB with coverage as alpha.
        /// </summary>
        private static Texture2D FontAtlas(Font font)
        {
            var source = font.material.mainTexture;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true) { name = "Label font atlas" };
            try
            {
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }
            var pixels = copy.GetPixels32();
            byte minA = 255, maxA = 0;
            foreach (var p in pixels) { if (p.a < minA) minA = p.a; if (p.a > maxA) maxA = p.a; }
            // Depending on the graphics API, coverage arrives in alpha or in red.
            var useAlpha = maxA - minA > 16;
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, useAlpha ? pixels[i].a : pixels[i].r);
            copy.SetPixels32(pixels);
            copy.Apply(false);
            copy.filterMode = FilterMode.Bilinear;
            return copy;
        }

        private static Color Hex(float grey) => new Color(grey, grey, grey);
        private static Color Hex(string hex) => StylePalette.Hex(hex);
        private static Color Lighten(Color color, float amount) => Color.Lerp(color, Color.white, amount);
        private static Color Darken(Color color, float amount) => Color.Lerp(color, Color.black, amount);
    }
}
