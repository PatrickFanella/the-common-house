using System;
using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>The generated material library used by the style studies.</summary>
    internal sealed class StyleMaterials
    {
        private readonly int size;
        public readonly List<(string Name, Material Material)> Library = new List<(string, Material)>();

        public Material Timber, DarkTimber, WeatheredTimber, Floorboards, Plaster, KitchenTiles, WallTiles;
        public Material RadioPaint, UtilityPaint, TealPaint, CreamEnamel, RedEnamel, MustardPaint, PegboardPaint;
        public Material Polymer, Copper, Brass, Steel, Paper, OldPaper, Cardboard, Tape, Tablecloth, Cork, Cable, RedCable, ClothTape;
        public Material Glass, Bulb, Dial, Indicator, NightWindow, Tube, Ink, RedInk, Pencil;

        public StyleMaterials(int textureSize)
        {
            size = textureSize;
            Timber = Make("Honey timber", 1.2f, 1f, (u, v) => ProceduralSurfaces.Timber(u, v, StylePalette.HoneyTimber, StylePalette.DarkTimber, 11));
            DarkTimber = Make("Dark timber", 1.2f, 1f, (u, v) => ProceduralSurfaces.Timber(u, v, Lighten(StylePalette.DarkTimber, 0.25f), Darken(StylePalette.DarkTimber, 0.4f), 23));
            WeatheredTimber = Make("Weathered timber", 1.4f, 1f, (u, v) => ProceduralSurfaces.Timber(u, v, StylePalette.GreyTimber, Darken(StylePalette.GreyTimber, 0.45f), 37));
            Floorboards = Make("Floorboards", 1.6f, 0.8f, (u, v) => ProceduralSurfaces.Floorboards(u, v, StylePalette.HoneyTimber, StylePalette.DarkTimber, 41));
            Plaster = Make("Plaster", 0.25f, 0.5f, (u, v) => ProceduralSurfaces.Plaster(u, v, StylePalette.PlasterCream, 53));
            KitchenTiles = Make("Kitchen floor tiles", 1.5f, 0.6f, (u, v) => Tiles(u, v, StylePalette.EnamelCream, StylePalette.KitchenTeal, Hex(0.3f), 4, 61));
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
            Bulb = Emissive("Warm bulb", new Color(1f, 0.78f, 0.5f), 2.2f);
            Dial = Emissive("Amber dial", StylePalette.DialAmber, 1.1f);
            Indicator = Emissive("Cyan indicator", StylePalette.IndicatorCyan, 1.4f);
            NightWindow = Emissive("Night window", new Color(0.1f, 0.14f, 0.24f), 0.6f);
            Tube = Emissive("Cool tube", new Color(0.85f, 0.93f, 1f), 1.6f);
            Ink = Text("Ink", StylePalette.Ink);
            RedInk = Text("Red ink", StylePalette.RedInk);
            Pencil = Text("Pencil", StylePalette.PencilGrey);
        }

        private Material Make(string name, float normalStrength, float tiling, Func<float, float, SurfaceSample> sample)
        {
            var surface = Build(name, size, normalStrength * size / 256f, sample);
            var material = new Material(Shader.Find("Standard")) { name = name };
            material.SetTexture("_MainTex", surface.Albedo);
            material.SetTexture("_BumpMap", surface.Normal);
            material.SetFloat("_BumpScale", 1f);
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap", surface.MetallicGloss);
            material.SetFloat("_GlossMapScale", 1f);
            material.EnableKeyword("_METALLICGLOSSMAP");
            material.mainTextureScale = new Vector2(tiling, tiling);
            Library.Add((name, material));
            return material;
        }

        private Material Emissive(string name, Color color, float intensity)
        {
            var material = new Material(Shader.Find("Standard")) { name = name, color = color * 0.3f };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return material;
        }

        private static Material Transparent(string name, Color color)
        {
            var material = new Material(Shader.Find("Standard")) { name = name, color = color };
            material.SetFloat("_Mode", 3f);
            material.SetFloat("_Glossiness", 0.9f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            return material;
        }

        private static Material Text(string name, Color ink)
        {
            var shader = Shader.Find("CommonHouse/LitText");
            if (shader == null) throw new InvalidOperationException("Missing CommonHouse/LitText shader.");
            return new Material(shader) { name = name, color = ink };
        }

        private static Color Hex(float grey) => new Color(grey, grey, grey);
        private static Color Hex(string hex) => StylePalette.Hex(hex);
        private static Color Lighten(Color color, float amount) => Color.Lerp(color, Color.white, amount);
        private static Color Darken(Color color, float amount) => Color.Lerp(color, Color.black, amount);
    }
}
