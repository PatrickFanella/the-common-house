using System;
using UnityEngine;

namespace CommonHouse.Interaction
{
    /// <summary>Shared temporary IMGUI scale, panel, and keyboard-scroll behavior for essential case text.</summary>
    public static class CaseUi
    {
        public const float MinimumTextScale = 1f;
        public const float MaximumTextScale = 1.5f;
        private static float textScale = MinimumTextScale;
        private static int lastScaleFrame = -1;
        private static int lastScrollFrame = -1;
        public static float TextScale
        {
            get => textScale;
            set => textScale = Mathf.Clamp(value, MinimumTextScale, MaximumTextScale);
        }

        public static GUIStyle WrappedLabel()
        {
            return CreateWrappedStyle(GUI.skin.label);
        }

        public static GUIStyle CreateWrappedStyle(GUIStyle baseStyle) => new GUIStyle(baseStyle) { wordWrap = true, fontSize = Mathf.RoundToInt(16f * TextScale) };

        public static GUIStyle Label() => new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(16f * TextScale) };

        public static Rect Panel(float x = .08f, float y = .08f, float width = .84f, float height = .84f)
        {
            return PanelForResolution(Screen.width, Screen.height, x, y, width, height);
        }

        public static Rect PanelForResolution(float screenWidth, float screenHeight, float x = .08f, float y = .08f, float width = .84f, float height = .84f)
        {
            var margin = 12f;
            return new Rect(Mathf.Max(margin, screenWidth * x), Mathf.Max(margin, screenHeight * y),
                Mathf.Min(screenWidth - margin * 2f, screenWidth * width), Mathf.Min(screenHeight - margin * 2f, screenHeight * height));
        }

        public static void DrawOpaquePanel(Rect panel, string title = null)
        {
            var previous = GUI.color;
            GUI.color = new Color(.04f, .05f, .08f, 1f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = previous;
            if (title != null) GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 28f), title, Label());
        }

        public static void HandleKeyboardScale()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            if (lastScaleFrame == Time.frameCount) return;
            lastScaleFrame = Time.frameCount;
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) TextScale += .1f;
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) TextScale -= .1f;
        }

        public static Vector2 Scroll(Vector2 value, float page = 120f)
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return value;
            if (lastScrollFrame == Time.frameCount) return value;
            lastScrollFrame = Time.frameCount;
            if (Input.GetKeyDown(KeyCode.PageDown)) value = ApplyScrollKey(value, KeyCode.PageDown, page);
            if (Input.GetKeyDown(KeyCode.PageUp)) value = ApplyScrollKey(value, KeyCode.PageUp, page);
            if (Input.GetKeyDown(KeyCode.Home)) value = ApplyScrollKey(value, KeyCode.Home, page);
            if (Input.GetKeyDown(KeyCode.End)) value = ApplyScrollKey(value, KeyCode.End, page);
            return value;
        }

        public static Vector2 ApplyScrollKey(Vector2 value, KeyCode key, float page = 120f)
        {
            if (key == KeyCode.PageDown) value.y += page * TextScale;
            if (key == KeyCode.PageUp) value.y = Mathf.Max(0f, value.y - page * TextScale);
            if (key == KeyCode.Home) value.y = 0f;
            if (key == KeyCode.End) value.y = 1000000f;
            return value;
        }
    }
}
