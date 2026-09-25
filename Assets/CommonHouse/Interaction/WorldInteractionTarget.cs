using UnityEngine;

namespace CommonHouse.Interaction
{
    /// <summary>A focused scene action, dispatched by the existing inspection ray and input owner.</summary>
    public abstract class WorldInteractionTarget : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract bool Interact();
    }
}
