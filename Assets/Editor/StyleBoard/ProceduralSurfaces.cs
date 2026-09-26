using System;
using UnityEngine;

namespace CommonHouse.Editor.StyleBoard
{
    /// <summary>One texel of a generated surface before it is packed into textures.</summary>
    internal struct SurfaceSample
    {
        public Color Albedo;
        public float Height;
        public float Metallic;
        public float Smoothness;

        public SurfaceSample(Color albedo, float height, float metallic, float smoothness)
        {
            Albedo = albedo;
            Height = height;
            Metallic = metallic;
            Smoothness = smoothness;
        }
    }

    /// <summary>Albedo, normal, and HDRP mask textures (metallic, occlusion, detail mask, smoothness).</summary>
    internal sealed class Surface
    {
        public Texture2D Albedo;
        public Texture2D Normal;
        public Texture2D Mask;
    }

    /// <summary>
    /// Deterministic, tileable surface generators. Integer seeds and a fixed hash make every
    /// run produce identical textures; no Unity random state or imported file is involved.
    /// </summary>
    internal static class ProceduralSurfaces
    {
        public static Surface Build(string name, int size, float normalStrength, Func<float, float, SurfaceSample> sample)
        {
            var albedo = new Color32[size * size];
            var metal = new Color32[size * size];
            var height = new float[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var s = sample((x + 0.5f) / size, (y + 0.5f) / size);
                var i = y * size + x;
                albedo[i] = s.Albedo;
                height[i] = s.Height;
                metal[i] = new Color(Mathf.Clamp01(s.Metallic), 1f, 0f, Mathf.Clamp01(s.Smoothness));
            }
            var normal = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = height[y * size + (x + size - 1) % size] - height[y * size + (x + 1) % size];
                var dy = height[((y + size - 1) % size) * size + x] - height[((y + 1) % size) * size + x];
                var n = new Vector3(dx * normalStrength, dy * normalStrength, 1f).normalized;
                normal[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
            }
            return new Surface
            {
                Albedo = Pack(name + "-albedo", size, albedo, false),
                Normal = Pack(name + "-normal", size, normal, true),
                Mask = Pack(name + "-mask", size, metal, true),
            };
        }

        private static Texture2D Pack(string name, int size, Color32[] pixels, bool linear)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, linear)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 8,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true);
            return texture;
        }

        // ---- Noise -------------------------------------------------------------------------

        private static float Lattice(int x, int y, int seed)
        {
            unchecked
            {
                var h = (uint)x * 0x8da6b343u ^ (uint)y * 0xd8163841u ^ (uint)seed * 0xcb1ab31fu;
                h ^= h >> 15;
                h *= 0x2c1b3c6du;
                h ^= h >> 12;
                h *= 0x297a2d39u;
                h ^= h >> 15;
                return (h & 0xffffff) / 16777215f;
            }
        }

        private static int Wrap(int value, int period) => ((value % period) + period) % period;

        /// <summary>Smooth value noise that tiles over [0,1) with integer frequencies.</summary>
        public static float Value(float u, float v, int fx, int fy, int seed)
        {
            var x = u * fx;
            var y = v * fy;
            var x0 = Mathf.FloorToInt(x);
            var y0 = Mathf.FloorToInt(y);
            var tx = x - x0;
            var ty = y - y0;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            int ax = Wrap(x0, fx), bx = Wrap(x0 + 1, fx), ay = Wrap(y0, fy), by = Wrap(y0 + 1, fy);
            var top = Mathf.Lerp(Lattice(ax, ay, seed), Lattice(bx, ay, seed), tx);
            var bottom = Mathf.Lerp(Lattice(ax, by, seed), Lattice(bx, by, seed), tx);
            return Mathf.Lerp(top, bottom, ty);
        }

        public static float Fbm(float u, float v, int fx, int fy, int octaves, int seed)
        {
            float sum = 0f, amplitude = 0.5f, total = 0f;
            for (var octave = 0; octave < octaves; octave++)
            {
                sum += Value(u, v, fx, fy, seed + octave * 31) * amplitude;
                total += amplitude;
                amplitude *= 0.5f;
                fx *= 2;
                fy *= 2;
            }
            return sum / total;
        }

        private static float Smooth(float edge0, float edge1, float x) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge0, edge1, x));

        private static float Frac(float x) => x - Mathf.Floor(x);

        private static Color Scale(Color color, float amount) => new Color(color.r * amount, color.g * amount, color.b * amount, 1f);

        // ---- Materials ---------------------------------------------------------------------

        /// <summary>Paint over a substrate, chipped at irregular patches with a primer rim and rust on metal.</summary>
        public static SurfaceSample Painted(float u, float v, Color paint, Color substrate, float substrateMetallic, float chipThreshold, int seed)
        {
            var chip = Fbm(u, v, 6, 6, 5, seed);
            var bare = Smooth(chipThreshold, chipThreshold + 0.025f, chip);
            var rim = Smooth(chipThreshold - 0.05f, chipThreshold, chip) * (1f - bare);
            var mottle = 0.9f + 0.16f * Fbm(u, v, 3, 3, 3, seed + 3);
            var grime = Fbm(u, v, 2, 2, 3, seed + 13);
            var color = Scale(paint, mottle * (0.85f + 0.15f * grime));
            color = Color.Lerp(color, new Color(0.45f, 0.27f, 0.2f), rim * 0.6f);
            var baseColor = substrate;
            var rust = 0f;
            if (substrateMetallic > 0f)
            {
                rust = Smooth(0.45f, 0.62f, Fbm(u, v, 5, 5, 4, seed + 9));
                baseColor = Color.Lerp(substrate, new Color(0.4f, 0.2f, 0.1f), rust);
            }
            color = Color.Lerp(color, baseColor, bare);
            var metallic = bare * substrateMetallic * (1f - rust);
            var smooth = Mathf.Lerp(0.42f * (0.7f + 0.3f * grime), 0.35f * (1f - rust), bare);
            return new SurfaceSample(color, 1f - 0.6f * bare, metallic, smooth);
        }

        /// <summary>Square tiles in an alternating pattern with worn glaze and grout.</summary>
        public static SurfaceSample Tiles(float u, float v, Color a, Color b, Color grout, int count, int seed)
        {
            var tx = Mathf.FloorToInt(u * count);
            var ty = Mathf.FloorToInt(v * count);
            var lx = Frac(u * count);
            var ly = Frac(v * count);
            var edge = Mathf.Min(Mathf.Min(lx, 1f - lx), Mathf.Min(ly, 1f - ly));
            var inside = Smooth(0.025f, 0.05f, edge);
            var wear = Fbm(u, v, 4, 4, 4, seed);
            var tile = ((tx + ty) & 1) == 0 ? a : b;
            tile = Scale(tile, 0.9f + 0.18f * Lattice(tx, ty, seed + 1) - 0.12f * wear);
            var color = Color.Lerp(Scale(grout, 0.8f + 0.3f * wear), tile, inside);
            return new SurfaceSample(color, inside * (0.8f + 0.2f * Smooth(0f, 0.2f, edge)), 0f, Mathf.Lerp(0.08f, 0.6f - 0.3f * wear, inside));
        }

        /// <summary>Fibrous paper with foxing spots.</summary>
        public static SurfaceSample Paper(float u, float v, Color baseColor, int seed)
        {
            var fibre = Fbm(u, v, 4, 64, 2, seed) * 0.5f + Fbm(u, v, 16, 16, 3, seed + 1) * 0.5f;
            var fox = Smooth(0.68f, 0.78f, Fbm(u, v, 6, 6, 4, seed + 4));
            var color = Scale(baseColor, 0.94f + 0.1f * fibre);
            color = Color.Lerp(color, new Color(0.62f, 0.48f, 0.32f), fox * 0.45f);
            return new SurfaceSample(color, fibre * 0.4f, 0f, 0.1f);
        }

        /// <summary>Woven check cloth.</summary>
        public static SurfaceSample Check(float u, float v, Color a, Color b, int bands, int seed)
        {
            var stripeU = Frac(u * bands) < 0.5f ? 1f : 0f;
            var stripeV = Frac(v * bands) < 0.5f ? 1f : 0f;
            var weave = Mathf.Sin(u * Mathf.PI * 2f * 160f) * Mathf.Sin(v * Mathf.PI * 2f * 160f);
            var fade = Fbm(u, v, 3, 3, 3, seed);
            var color = Color.Lerp(a, b, (stripeU + stripeV) * 0.5f);
            color = Color.Lerp(color, Color.Lerp(a, Color.white, 0.4f), fade * 0.25f);
            color = Scale(color, 0.92f + 0.08f * weave);
            return new SurfaceSample(color, 0.5f + 0.5f * weave, 0f, 0.04f);
        }

        /// <summary>Moulded polymer with fine scratches, used for surviving newer equipment.</summary>
        public static SurfaceSample Polymer(float u, float v, Color baseColor, int seed)
        {
            var scratch = Smooth(0.82f, 0.9f, Value(u, v, 3, 220, seed)) * Smooth(0.5f, 0.7f, Fbm(u, v, 4, 4, 2, seed + 1));
            var mottle = Fbm(u, v, 2, 2, 3, seed + 2);
            var color = Scale(baseColor, 0.95f + 0.1f * mottle + 0.8f * scratch);
            return new SurfaceSample(color, 1f - scratch, 0f, 0.62f - 0.35f * scratch);
        }

        /// <summary>Bare metal with patches of tarnish or patina.</summary>
        public static SurfaceSample Tarnished(float u, float v, Color metal, Color patina, int seed)
        {
            var t = Smooth(0.66f, 0.78f, Fbm(u, v, 4, 4, 5, seed)) * 0.8f;
            var brushed = Fbm(u, v, 2, 128, 2, seed + 3);
            var color = Color.Lerp(Scale(metal, 0.9f + 0.15f * brushed), patina, t);
            return new SurfaceSample(color, brushed * 0.3f + t * 0.4f, Mathf.Lerp(0.95f, 0.2f, t), Mathf.Lerp(0.6f, 0.2f, t));
        }

        /// <summary>Speckled cork for a pin board.</summary>
        public static SurfaceSample Cork(float u, float v, int seed)
        {
            var coarse = Value(u, v, 90, 90, seed);
            var fine = Value(u, v, 190, 190, seed + 1);
            var t = Mathf.Clamp01(coarse * 0.6f + fine * 0.5f);
            var color = Color.Lerp(new Color(0.36f, 0.24f, 0.14f), new Color(0.72f, 0.54f, 0.34f), t);
            return new SurfaceSample(color, t, 0f, 0.05f);
        }

        /// <summary>Painted pegboard with a regular hole grid and faint painted tool outlines.</summary>
        public static SurfaceSample Pegboard(float u, float v, Color paint, int holes, int seed)
        {
            var baseSample = Painted(u, v, paint, new Color(0.55f, 0.42f, 0.28f), 0f, 0.8f, seed);
            var lx = Frac(u * holes) - 0.5f;
            var ly = Frac(v * holes) - 0.5f;
            var hole = 1f - Smooth(0.1f, 0.14f, Mathf.Sqrt(lx * lx + ly * ly));
            baseSample.Albedo = Color.Lerp(baseSample.Albedo, new Color(0.03f, 0.03f, 0.03f), hole);
            baseSample.Height = 1f - hole;
            return baseSample;
        }

        /// <summary>Plain rubber or cloth-wrapped cable.</summary>
        public static SurfaceSample Rubber(float u, float v, Color baseColor, int seed)
        {
            var wrap = Mathf.Sin(u * Mathf.PI * 2f * 40f + v * 20f);
            var dust = Fbm(u, v, 3, 3, 3, seed);
            return new SurfaceSample(Scale(baseColor, 0.9f + 0.1f * wrap + 0.15f * dust), 0.5f + 0.5f * wrap, 0f, 0.3f - 0.1f * dust);
        }
    }
}
