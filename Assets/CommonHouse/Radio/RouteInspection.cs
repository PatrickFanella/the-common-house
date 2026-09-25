using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHouse.Radio
{
    public enum RouteInspectionResult { Verified, AlreadyVerified, Rejected }

    public sealed class RouteRecipientRecord
    {
        public RouteRecipientRecord(string routeName, IEnumerable<string> recipients)
        {
            RouteName = routeName;
            Recipients = Array.AsReadOnly((recipients ?? throw new ArgumentNullException(nameof(recipients))).OrderBy(id => id, StringComparer.Ordinal).ToArray());
        }

        public string RouteName { get; }
        public IReadOnlyList<string> Recipients { get; }
    }

    /// <summary>Fixed, readable routing facts. No switch position determines an ending.</summary>
    public sealed class RouteInspectionModel
    {
        public static readonly RouteRecipientRecord PrivateHeadset = new RouteRecipientRecord("Private headset", new[] { "ada", "ren" });
        public static readonly RouteRecipientRecord RoomSpeaker = new RouteRecipientRecord("Room speaker", new[] { "ivo", "player", "ren" });

        public bool DiagramRead { get; private set; }
        public bool ConnectionRead { get; private set; }
        public bool CanVerify => DiagramRead && ConnectionRead;

        public void ReadDiagram() => DiagramRead = true;
        public void ReadConnection() => ConnectionRead = true;

        public void Reset()
        {
            DiagramRead = false;
            ConnectionRead = false;
        }
    }

    public readonly struct RouteInspectionInputFrame
    {
        public RouteInspectionInputFrame(bool previous, bool next, bool read, bool verify)
        {
            Previous = previous;
            Next = next;
            Read = read;
            Verify = verify;
        }

        public bool Previous { get; }
        public bool Next { get; }
        public bool Read { get; }
        public bool Verify { get; }
    }

}
