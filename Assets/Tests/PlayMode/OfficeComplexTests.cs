using System;
using System.Collections;
using System.IO;
using System.Linq;
using CommonHouse.Outpost;
using CommonHouse.Opening;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class OfficeComplexTests
    {
        private FirstPersonMovement player;
        private OutpostLayout layout;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<OpeningHouseholdController>().Close();
            player = Object.FindAnyObjectByType<FirstPersonMovement>();
            player.SetInputSource(new FixedInput());
            layout = Object.FindAnyObjectByType<OutpostLayout>();
            Physics.SyncTransforms();
        }

        [Test]
        public void ConversionHasDistinctOfficeSpacesAndPreservesCaseAnchors()
        {
            var conversion = layout.transform.Find("OfficeConversion");
            foreach (var room in new[] { "OfficeConcourse", "RepairHackerspace", "ReceptionCommons", "CommunalSleeping", "UtilityRoom" })
            {
                var space = conversion.Find(room);
                Assert.That(space, Is.Not.Null, room);
                Assert.That(space.Find("Office floor").GetComponent<Collider>(), Is.Not.Null);
                Assert.That(space.Find("Office suspended ceiling"), Is.Not.Null);
            }
            Assert.That(layout.Anchors.Select(anchor => anchor.AnchorId), Is.EquivalentTo(OutpostLayout.RequiredAnchorIds));
            Assert.That(layout.GetAnchor("radio-console").transform.position, Is.EqualTo(new Vector3(-11.5f, 0.5f, -2.5f)));
            Assert.That(layout.GetAnchor("workshop-bench").transform.position, Is.EqualTo(new Vector3(11.5f, 0.5f, 2.5f)));
            Assert.That(layout.ContainsPresentDayPoint(new Vector3(9.5f, 1f, -14f)), Is.True);
            Assert.That(layout.ContainsPresentDayPoint(new Vector3(0f, 1f, -18f)), Is.False);
            Assert.That(layout.ContainsPresentDayPoint(new Vector3(0f, 5f, -12f)), Is.False);
        }

        [Test]
        public void ActualPlayerWalksFromCaseRoomsThroughAllNewDestinationsWithoutGrantingEvidence()
        {
            // Every segment is walked with CharacterController integration, with no teleports.
            foreach (var point in new[]
            {
                new Vector2(-10f, -6f), new Vector2(-9.5f, -6f), new Vector2(-9.5f, -12f),
                new Vector2(-9.5f, -6f), new Vector2(0f, -6f), new Vector2(0f, -10f),
                new Vector2(0f, -6f), new Vector2(9.5f, -6f), new Vector2(9.5f, -10f), new Vector2(9.5f, -14f),
                new Vector2(9.5f, -6f), new Vector2(11.5f, -6f), new Vector2(11.5f, 0f), new Vector2(0f, 0f), new Vector2(-10f, 0f),
            }) WalkTo(point);
            var state = Object.FindAnyObjectByType<CaseSession>().State;
            foreach (var evidence in new[] { "E01", "E02", "E03", "E04", "E05", "E06" })
                Assert.That(state.HasEvidence(evidence), Is.False, "Walking must not acquire " + evidence);
        }

        [Test]
        public void FrontAndSideCollisionBoundariesHoldTheRealPlayerInsideTheGroundFloor()
        {
            WalkTo(new Vector2(-10f, -6f));
            WalkTo(new Vector2(9.5f, -6f));
            WalkTo(new Vector2(9.5f, -14f));
            for (var step = 0; step < 60; step++) player.Tick(new FirstPersonInputFrame(Vector2.down, Vector2.zero), 0.05f);
            Assert.That(player.transform.position.z, Is.GreaterThanOrEqualTo(-15.56f));
            WalkTo(new Vector2(9.5f, -13.5f));
            WalkTo(new Vector2(9.5f, -6f));
            for (var step = 0; step < 60; step++) player.Tick(new FirstPersonInputFrame(Vector2.right, Vector2.zero), 0.05f);
            Assert.That(player.transform.position.x, Is.InRange(13f, 13.56f));
            Assert.That(player.transform.position.y, Is.InRange(-0.1f, 0.2f), "No route may fall through a floor seam.");
        }

        [Test]
        public void OriginalPropKitAndOfficeShellUseTheChosenRetroMaterials()
        {
            var conversion = layout.transform.Find("OfficeConversion");
            var names = conversion.GetComponentsInChildren<Transform>().Select(item => item.name).ToArray();
            foreach (var name in new[] { "Patched terminal", "Shared repair toolboard", "Rewired service rack", "Communal sleeping cot", "Reused office locker", "Mismatched common sofa", "Converted reception counter", "Salvaged battery bank" })
                Assert.That(names, Does.Contain(name));
            foreach (var renderer in conversion.GetComponentsInChildren<Renderer>())
            foreach (var material in renderer.sharedMaterials)
                Assert.That(material.shader.name, Is.EqualTo("CommonHouse/Retro"), renderer.name);
            var shell = layout.transform.Find("OfficeExteriorMassing");
            Assert.That(shell, Is.Not.Null);
            Assert.That(shell.GetComponentsInChildren<Transform>().Count(item => item.name == "Pale masonry wing"), Is.EqualTo(2));
            Assert.That(shell.GetComponentsInChildren<Transform>().Count(item => item.name == "East facade fin"), Is.GreaterThan(8));
            Assert.That(shell.GetComponentsInChildren<Transform>().Count(item => item.name == "Upper curtain glass band"), Is.EqualTo(5));
            Assert.That(conversion.GetComponentsInChildren<Transform>().Single(item => item.name == "Closed upper-floor access").GetComponent<Collider>(), Is.Null,
                "Upper-floor access is visual dressing on a solid wall, not an open route.");
        }

        [UnityTest]
        public IEnumerator RenderConvertedOfficeSpacesAndExteriorShell()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            var camera = player.GetComponentInChildren<Camera>();
            foreach (var view in new[]
            {
                ("office-concourse", new Vector3(-11.5f, 1.6f, -6f), new Vector3(5f, 1.6f, -6f)),
                ("repair-hackerspace", new Vector3(-7.8f, 1.6f, -8.8f), new Vector3(-11.2f, 1.4f, -13f)),
                ("reception-commons", new Vector3(0f, 1.6f, -8.5f), new Vector3(0f, 1.2f, -14f)),
                ("communal-sleeping", new Vector3(9.5f, 1.6f, -8.5f), new Vector3(12f, 0.9f, -10.5f)),
                ("utility-room", new Vector3(9.5f, 1.6f, -12.5f), new Vector3(8.2f, 1f, -15f)),
                ("office-exterior", new Vector3(36f, 16f, -43f), new Vector3(0f, 10f, -7f)),
            })
            {
                camera.transform.position = view.Item2;
                camera.transform.LookAt(view.Item3);
                yield return null; yield return null;
                Capture(camera, view.Item1);
            }
        }

        private void WalkTo(Vector2 destination)
        {
            for (var step = 0; step < 250; step++)
            {
                var position = new Vector2(player.transform.position.x, player.transform.position.z);
                var delta = destination - position;
                if (delta.magnitude < 0.15f) break;
                player.Tick(new FirstPersonInputFrame(delta.normalized, Vector2.zero), Mathf.Min(0.05f, delta.magnitude / player.MovementSpeed));
            }
            Assert.That(Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), destination), Is.LessThan(0.2f),
                "Player blocked on route to " + destination + " at " + player.transform.position);
            Assert.That(player.transform.position.y, Is.InRange(-0.1f, 0.2f));
        }

        internal static void Capture(Camera camera, string name)
        {
            var directory = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            Assert.That(directory, Is.Not.Null.And.Not.Empty);
            var render = new RenderTexture(1000, 700, 24);
            var pixels = new Texture2D(1000, 700, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = render;
                for (var frame = 0; frame < 3; frame++) camera.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, 1000, 700), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(render);
                Object.DestroyImmediate(pixels);
            }
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
