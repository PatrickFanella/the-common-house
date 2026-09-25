using System.Collections;
using CommonHouse.Outpost;
using CommonHouse.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class PlayerMovementTests
    {
        private GameObject player;
        private GameObject view;
        private FirstPersonMovement controller;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Test Player");
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            view = new GameObject("Test Player View");
            view.transform.SetParent(player.transform, false);
            view.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            controller = player.AddComponent<FirstPersonMovement>();
            controller.SetViewTransform(view.transform);
            controller.SetInputSource(new FixedInput());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(view);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void DefaultsKeepHeadBobAndCameraShakeDisabled()
        {
            Assert.That(controller.HeadBobEnabled, Is.False);
            Assert.That(controller.CameraShakeEnabled, Is.False);
        }

        [Test]
        public void TestInputMovesAndTurnsWithoutCapturingTheHostPointer()
        {
            var before = player.transform.position;
            controller.MouseSensitivity = 30f;
            controller.Tick(new FirstPersonInputFrame(new Vector2(0f, 1f), new Vector2(1f, -1f)), 0.25f);

            Assert.That(player.transform.position.z, Is.GreaterThan(before.z));
            Assert.That(Mathf.DeltaAngle(0f, player.transform.eulerAngles.y), Is.EqualTo(30f).Within(0.01f));
            Assert.That(Mathf.DeltaAngle(0f, view.transform.localEulerAngles.x), Is.EqualTo(30f).Within(0.01f));
            Assert.That(controller.PointerCaptured, Is.False);
        }

        [UnityTest]
        public IEnumerator CharacterControllerStopsAtACollisionBoundary()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(12f, 0.2f, 12f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(2f, 1.5f, 0f);
            wall.transform.localScale = new Vector3(0.3f, 3f, 4f);
            try
            {
                Physics.SyncTransforms();
                for (var frame = 0; frame < 20; frame++)
                    controller.Tick(new FirstPersonInputFrame(new Vector2(1f, 0f), Vector2.zero), 0.1f);

                Assert.That(player.transform.position.x, Is.GreaterThan(1f));
                Assert.That(player.transform.position.x, Is.LessThanOrEqualTo(1.56f));
                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(floor);
                Object.DestroyImmediate(wall);
            }
        }

        [Test]
        public void PauseOrMenuFocusReleasesPointerAndSuppressesMovement()
        {
            var before = player.transform.position;
            controller.SetMenuOpen(true);
            controller.Tick(new FirstPersonInputFrame(new Vector2(0f, 1f), Vector2.zero), 1f);

            Assert.That(player.transform.position, Is.EqualTo(before));
            Assert.That(controller.PointerCaptured, Is.False);

            controller.SetMenuOpen(false);
            controller.Tick(new FirstPersonInputFrame(new Vector2(0f, 1f), Vector2.zero), 0.25f);
            Assert.That(player.transform.position.z, Is.GreaterThan(before.z));
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostPlayerTraversesConnectedRoomsButCannotExitTheBoundary()
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("OutpostBlockout", UnityEngine.SceneManagement.LoadSceneMode.Single);
            Physics.SyncTransforms();
            var outpostPlayer = Object.FindAnyObjectByType<FirstPersonMovement>();
            Assert.That(outpostPlayer, Is.Not.Null, "The generated outpost must include the first-person player.");
            Assert.That(outpostPlayer.GetComponent<CharacterController>(), Is.Not.Null);
            outpostPlayer.SetInputSource(new FixedInput());

            for (var frame = 0; frame < 60; frame++)
                outpostPlayer.Tick(new FirstPersonInputFrame(new Vector2(1f, 0f), Vector2.zero), 0.1f);
            Assert.That(outpostPlayer.transform.position.x, Is.GreaterThan(9f),
                "The player must be able to cross the radio, kitchen, and archive route.");

            for (var frame = 0; frame < 60; frame++)
                outpostPlayer.Tick(new FirstPersonInputFrame(new Vector2(1f, 0f), Vector2.zero), 0.1f);
            Assert.That(outpostPlayer.transform.position.x, Is.LessThanOrEqualTo(13.56f),
                "The player must stop at the generated exterior wall.");
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
