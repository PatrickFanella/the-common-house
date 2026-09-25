using UnityEngine;

namespace CommonHouse.Player
{
    /// <summary>Digital keyboard look equivalent for play without pointer input.</summary>
    public static class KeyboardLookBindings
    {
        public const float UnitsPerSecond = 60f;

        public static Vector2 Resolve(bool left, bool right, bool up, bool down)
        {
            return new Vector2((right ? 1f : 0f) - (left ? 1f : 0f),
                (up ? 1f : 0f) - (down ? 1f : 0f));
        }

        public static Vector2 Resolve(bool left, bool right, bool up, bool down, float deltaTime)
        {
            return Resolve(left, right, up, down) * (UnitsPerSecond * Mathf.Max(0f, deltaTime));
        }
    }
}
