using System.Collections.Generic;
using UnityEngine;

namespace CommonHouse.Editor.StyleBoard
{
    /// <summary>
    /// Meshes with UVs measured in metres, so textures keep a consistent scale regardless of
    /// object size. Primitive cubes stretch their texture over each face instead.
    /// </summary>
    internal static class StyleMeshes
    {
        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        public static Mesh Box(Vector3 size)
        {
            var key = "box" + size.ToString("F4");
            if (Cache.TryGetValue(key, out var cached)) return cached;
            var builder = new Builder();
            var half = size * 0.5f;
            foreach (var normal in new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back })
            {
                var a = Mathf.Abs(normal.y) > 0.5f || Mathf.Abs(normal.z) > 0.5f ? Vector3.right : Vector3.forward;
                var b = Vector3.Cross(a, normal).normalized;
                var ea = Mathf.Abs(Vector3.Dot(a, half));
                var eb = Mathf.Abs(Vector3.Dot(b, half));
                var center = Vector3.Scale(normal, half);
                // Grain and print run along the longer side of each face.
                var swap = eb > ea;
                builder.Quad(center, normal, a * ea, b * eb, swap);
            }
            return Cache[key] = builder.Finish(key);
        }

        public static Mesh Cylinder(float radius, float height, int sides = 28)
        {
            var key = "cyl" + radius.ToString("F4") + "x" + height.ToString("F4") + "s" + sides;
            if (Cache.TryGetValue(key, out var cached)) return cached;
            var builder = new Builder();
            var y0 = -height * 0.5f;
            var y1 = height * 0.5f;
            var circumference = Mathf.PI * 2f * radius;
            for (var i = 0; i < sides; i++)
            {
                var t0 = i / (float)sides;
                var t1 = (i + 1) / (float)sides;
                var p0 = Ring(t0, radius);
                var p1 = Ring(t1, radius);
                builder.Add(new[]
                    {
                        new Vector3(p0.x, y0, p0.z), new Vector3(p0.x, y1, p0.z),
                        new Vector3(p1.x, y1, p1.z), new Vector3(p1.x, y0, p1.z)
                    },
                    new[] { p0.normalized, p0.normalized, p1.normalized, p1.normalized },
                    new[]
                    {
                        new Vector2(t0 * circumference, y0), new Vector2(t0 * circumference, y1),
                        new Vector2(t1 * circumference, y1), new Vector2(t1 * circumference, y0)
                    });
                builder.Triangle(new Vector3(0, y1, 0), new Vector3(p0.x, y1, p0.z), new Vector3(p1.x, y1, p1.z), Vector3.up);
                builder.Triangle(new Vector3(0, y0, 0), new Vector3(p1.x, y0, p1.z), new Vector3(p0.x, y0, p0.z), Vector3.down);
            }
            return Cache[key] = builder.Finish(key);
        }

        /// <summary>A round tube through Catmull-Rom interpolated control points.</summary>
        public static Mesh Tube(IList<Vector3> points, float radius, int sides = 8, int segmentsPerSpan = 8)
        {
            var path = new List<Vector3>();
            for (var i = 0; i < points.Count - 1; i++)
            {
                var p0 = points[Mathf.Max(i - 1, 0)];
                var p1 = points[i];
                var p2 = points[i + 1];
                var p3 = points[Mathf.Min(i + 2, points.Count - 1)];
                for (var s = 0; s < segmentsPerSpan; s++)
                {
                    var t = s / (float)segmentsPerSpan;
                    path.Add(0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t));
                }
            }
            path.Add(points[points.Count - 1]);
            var builder = new Builder();
            var reference = Vector3.up;
            var rings = new List<Vector3[]>();
            var normals = new List<Vector3[]>();
            var distance = 0f;
            var along = new List<float>();
            for (var i = 0; i < path.Count; i++)
            {
                var forward = (path[Mathf.Min(i + 1, path.Count - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
                if (Mathf.Abs(Vector3.Dot(forward, reference)) > 0.95f) reference = Vector3.right;
                var side = Vector3.Cross(forward, reference).normalized;
                var up = Vector3.Cross(side, forward).normalized;
                reference = up;
                var ring = new Vector3[sides + 1];
                var ringNormals = new Vector3[sides + 1];
                for (var s = 0; s <= sides; s++)
                {
                    var angle = s / (float)sides * Mathf.PI * 2f;
                    var n = side * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    ring[s] = path[i] + n * radius;
                    ringNormals[s] = n;
                }
                if (i > 0) distance += Vector3.Distance(path[i], path[i - 1]);
                rings.Add(ring);
                normals.Add(ringNormals);
                along.Add(distance);
            }
            var circumference = Mathf.PI * 2f * radius;
            for (var i = 0; i < rings.Count - 1; i++)
            for (var s = 0; s < sides; s++)
            {
                float u0 = s / (float)sides * circumference, u1 = (s + 1) / (float)sides * circumference;
                builder.Add(new[] { rings[i][s], rings[i + 1][s], rings[i + 1][s + 1], rings[i][s + 1] },
                    new[] { normals[i][s], normals[i + 1][s], normals[i + 1][s + 1], normals[i][s + 1] },
                    new[] { new Vector2(along[i], u0), new Vector2(along[i + 1], u0), new Vector2(along[i + 1], u1), new Vector2(along[i], u1) });
            }
            return builder.Finish("tube");
        }

        private static Vector3 Ring(float t, float radius)
        {
            var angle = t * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private sealed class Builder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<int> triangles = new List<int>();

            public void Quad(Vector3 center, Vector3 normal, Vector3 a, Vector3 b, bool swapUv)
            {
                var corners = new[] { center - a - b, center - a + b, center + a + b, center + a - b };
                var ad = a.normalized;
                var bd = b.normalized;
                var uv = new Vector2[4];
                for (var i = 0; i < 4; i++)
                {
                    var p = new Vector2(Vector3.Dot(corners[i], ad), Vector3.Dot(corners[i], bd));
                    uv[i] = swapUv ? new Vector2(p.y, p.x) : p;
                }
                Add(corners, new[] { normal, normal, normal, normal }, uv);
            }

            public void Add(Vector3[] corners, Vector3[] cornerNormals, Vector2[] cornerUvs)
            {
                var start = vertices.Count;
                vertices.AddRange(corners);
                normals.AddRange(cornerNormals);
                uvs.AddRange(cornerUvs);
                AddOriented(start, start + 1, start + 2);
                AddOriented(start, start + 2, start + 3);
            }

            public void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
            {
                var start = vertices.Count;
                vertices.AddRange(new[] { a, b, c });
                normals.AddRange(new[] { normal, normal, normal });
                uvs.AddRange(new[] { new Vector2(a.x, a.z), new Vector2(b.x, b.z), new Vector2(c.x, c.z) });
                AddOriented(start, start + 1, start + 2);
            }

            // Unity treats clockwise triangles as front faces; that orientation makes the
            // geometric cross product point along the outward normal.
            private void AddOriented(int a, int b, int c)
            {
                var face = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                var expected = normals[a] + normals[b] + normals[c];
                if (Vector3.Dot(face, expected) >= 0f) triangles.AddRange(new[] { a, b, c });
                else triangles.AddRange(new[] { a, c, b });
            }

            public Mesh Finish(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
