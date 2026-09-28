using System.IO;
using UnityEngine;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>Shared low-resolution render-and-save helper for the style experiments.</summary>
    public static class ExperimentCapture
    {
        /// <summary>Renders at the given size and saves it, plus a 1280-wide copy scaled with the given filter.</summary>
        public static void Render(Camera camera, int width, int height, string path, FilterMode upscale)
        {
            var previous = RenderTexture.active;
            var frame = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { filterMode = upscale };
            var displayHeight = 1280 * height / width;
            var display = width < 1280 ? new RenderTexture(1280, displayHeight, 0, RenderTextureFormat.ARGB32) : null;
            try
            {
                frame.Create();
                camera.targetTexture = frame;
                for (var i = 0; i < 3; i++) camera.Render();
                Save(frame, path);
                if (display != null)
                {
                    display.Create();
                    Graphics.Blit(frame, display);
                    Save(display, path.Replace(".png", "-display.png"));
                }
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                frame.Release();
                UnityEngine.Object.DestroyImmediate(frame);
                if (display != null)
                {
                    display.Release();
                    UnityEngine.Object.DestroyImmediate(display);
                }
            }
        }

        private static void Save(RenderTexture texture, string path)
        {
            RenderTexture.active = texture;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
