using System.Collections;
using System.Collections.Generic;
using CommonHouse.Interaction;
using CommonHouse.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class InspectionTests
    {
        private GameObject player;
        private GameObject target;
        private FirstPersonMovement movement;
        private ModalInputCoordinator modalInput;
        private InspectionController inspection;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Inspection Test Player");
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = Vector3.up * 0.9f;
            movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            var camera = new GameObject("Inspection Test Camera").AddComponent<Camera>();
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = Vector3.up * 1.6f;
            movement.SetViewTransform(camera.transform);
            modalInput = player.AddComponent<ModalInputCoordinator>();
            inspection = player.AddComponent<InspectionController>();
            inspection.Configure(camera, movement, modalInput, 2.5f);
            target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Readable Test Notice";
            target.transform.position = new Vector3(0f, 1.6f, 2f);
            target.transform.localScale = new Vector3(1f, 1f, 0.1f);
            target.AddComponent<InspectionTarget>().Configure("test-notice", "Test notice", "Readable transcript text.", "test-source");
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void FocusRequiresAnUnoccludedTargetInsideRange()
        {
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentFocus, Is.EqualTo(target.GetComponent<InspectionTarget>()));
            Assert.That(inspection.FocusedPrompt, Does.Contain("Test notice"));

            target.transform.position = new Vector3(0f, 1.6f, 3f);
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentFocus, Is.Null);

            target.transform.position = new Vector3(0f, 1.6f, 2f);
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.transform.position = new Vector3(0f, 1.6f, 1f);
            blocker.transform.localScale = new Vector3(1f, 1f, 0.1f);
            try
            {
                Physics.SyncTransforms();
                inspection.RefreshFocus();
                Assert.That(inspection.CurrentFocus, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(blocker);
            }
        }

        [Test]
        public void OpeningShowsTextAndCoordinatorCancelRestoresFocusAndMovement()
        {
            inspection.RefreshFocus();
            inspection.Tick(new InspectionInputFrame(true, false, false));
            Assert.That(inspection.IsOpen, Is.True);
            Assert.That(inspection.VisiblePanelText, Is.EqualTo("Readable transcript text."));
            Assert.That(modalInput.IsOwner(inspection), Is.True);
            Assert.That(movement.MenuOpen, Is.True);

            modalInput.HandleCancel();
            Assert.That(inspection.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(movement.MenuOpen, Is.False);
            Assert.That(inspection.CurrentFocus, Is.EqualTo(target.GetComponent<InspectionTarget>()));
        }

        [Test]
        public void OpeningIsReadOnlyAndExplicitAcquisitionIsIdempotent()
        {
            var ledger = new RecordingLedger();
            inspection.SetLedger(ledger);
            inspection.RefreshFocus();
            Assert.That(inspection.TryOpenFocused(), Is.True);
            Assert.That(ledger.Attempts, Is.EqualTo(0));
            inspection.Close();
            inspection.RefreshFocus();
            Assert.That(inspection.TryOpenFocused(), Is.True);
            Assert.That(ledger.Attempts, Is.EqualTo(0));

            Assert.That(inspection.TryAcquireOpen(), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            Assert.That(inspection.TryAcquireOpen(), Is.EqualTo(InspectionAcquisitionResult.AlreadyAcquired));
            Assert.That(ledger.Attempts, Is.EqualTo(2));
            Assert.That(ledger.AcquiredIds, Is.EquivalentTo(new[] { "test-source" }));
            Assert.That(inspection.LastActionMessage, Does.Contain("already"));
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostProvidesAReachableGenericReadableNotice()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            var controller = Object.FindAnyObjectByType<InspectionController>();
            var notice = Object.FindObjectsByType<InspectionTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Assert.That(controller, Is.Not.Null);
            Assert.That(notice, Has.Length.EqualTo(1));
            Assert.That(notice[0].InspectionId, Is.EqualTo("sample-shift-notice"));
            controller.RefreshFocus();
            Assert.That(controller.CurrentFocus, Is.EqualTo(notice[0]));
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }

        private sealed class RecordingLedger : IInspectionLedger
        {
            public int Attempts { get; private set; }
            public HashSet<string> AcquiredIds { get; } = new HashSet<string>();

            public InspectionAcquisitionResult TryAcquire(string acquisitionId)
            {
                Attempts++;
                return AcquiredIds.Add(acquisitionId) ? InspectionAcquisitionResult.Acquired : InspectionAcquisitionResult.AlreadyAcquired;
            }
        }
    }
}
