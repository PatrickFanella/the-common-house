using System;
using UnityEngine;

namespace CommonHouse.Outpost
{
    /// <summary>Stable interaction position within one named outpost room.</summary>
    public sealed class OutpostAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId;
        [SerializeField] private string roomId;

        public string AnchorId => anchorId;
        public string RoomId => roomId;

        public void Configure(string id, string room)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Anchor ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(room)) throw new ArgumentException("Room ID is required.", nameof(room));
            anchorId = id;
            roomId = room;
        }
    }
}
