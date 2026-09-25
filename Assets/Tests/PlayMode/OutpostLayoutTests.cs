using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Outpost;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class OutpostLayoutTests
    {
        private static readonly IReadOnlyDictionary<string, string> AnchorRooms =
            new Dictionary<string, string>
            {
                { "radio-console", OutpostLayout.RadioRoom },
                { "radio-receiver", OutpostLayout.RadioRoom },
                { "kitchen-table", OutpostLayout.Kitchen },
                { "archive-roster", OutpostLayout.ArchiveWorkshop },
                { "workshop-bench", OutpostLayout.ArchiveWorkshop },
                { "routing-switch", OutpostLayout.ArchiveWorkshop },
            };

        [UnityTest]
        public IEnumerator GeneratedOutpostContainsOnlyTheThreePresentDayRoomsAndStableAnchors()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            var layout = Object.FindAnyObjectByType<OutpostLayout>();
            Assert.That(layout, Is.Not.Null);

            var roomIds = layout.transform.Cast<Transform>()
                .Where(child => child.name == OutpostLayout.RadioRoom || child.name == OutpostLayout.Kitchen || child.name == OutpostLayout.ArchiveWorkshop)
                .Select(child => child.name)
                .ToArray();
            Assert.That(roomIds, Is.EquivalentTo(new[] { OutpostLayout.RadioRoom, OutpostLayout.Kitchen, OutpostLayout.ArchiveWorkshop }));
            Assert.That(layout.Anchors.Select(anchor => anchor.AnchorId), Is.EquivalentTo(OutpostLayout.RequiredAnchorIds));

            foreach (var expected in AnchorRooms)
            {
                var anchor = layout.GetAnchor(expected.Key);
                Assert.That(anchor, Is.Not.Null, expected.Key + " must remain a named interaction anchor.");
                Assert.That(anchor.RoomId, Is.EqualTo(expected.Value));
                Assert.That(anchor.transform.parent.name, Is.EqualTo(expected.Value));
                Assert.That(layout.ContainsPresentDayPoint(anchor.transform.position), Is.True);
            }
            Assert.That(layout.ContainsPresentDayPoint(new Vector3(15f, 1f, 0f)), Is.False, "No exterior traversal is part of this blockout.");
        }

        [UnityTest]
        public IEnumerator RadioKitchenAndArchiveTraversalPointsStayClearWhileExteriorWallsBlock()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            Physics.SyncTransforms();
            var points = OutpostLayout.TraversalPoints;
            Assert.That(points.Count, Is.EqualTo(3));

            for (var segment = 0; segment < points.Count - 1; segment++)
            {
                for (var step = 0; step <= 40; step++)
                {
                    var position = Vector3.Lerp(points[segment], points[segment + 1], step / 40f);
                    var blockers = CapsuleBlockers(position).ToArray();
                    Assert.That(blockers, Is.Empty,
                        "The radio-to-kitchen-to-archive route must stay clear at " + position + "; blockers: "
                        + string.Join(", ", blockers.Select(collider => collider.name)) + ".");
                }
            }

            Assert.That(CapsuleBlockers(new Vector3(13.9f, 1f, 0f)).Any(), Is.True,
                "The present-day outpost needs a collision boundary rather than an exterior route.");
        }

        private static IEnumerable<Collider> CapsuleBlockers(Vector3 center)
        {
            // `center` is a foot-level traversal point. The capsule spans a 1.8 m character.
            var overlaps = Physics.OverlapCapsule(center + Vector3.down * 0.64f, center + Vector3.up * 0.5f, 0.3f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            // The generated scene starts the future player controller at the radio point.
            // Its own CharacterController is not an obstacle in the static room geometry.
            return overlaps.Where(collider => !(collider is CharacterController) && collider.name != "Player");
        }
    }
}
