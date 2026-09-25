using UnityEngine;

namespace CommonHouse.Player
{
    /// <summary>Explicit WASD movement avoids sharing arrow keys with keyboard look and panels.</summary>
    public static class KeyboardMovementBindings
    {
        public static Vector2 Resolve(bool left, bool right, bool forward, bool backward)
        {
            return new Vector2((right ? 1f : 0f) - (left ? 1f : 0f),
                (forward ? 1f : 0f) - (backward ? 1f : 0f));
        }
    }
}
