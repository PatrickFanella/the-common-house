using CommonHouse.Content;
using System.Collections;
using System.Linq;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class RouteInspectionTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private RouteInspectionController routeInspection;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Route Inspection Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            session.Configure(CaseContent.LoadBaseline(), new CaseState(CaseContent.LoadBaseline()));
            routeInspection = player.AddComponent<RouteInspectionController>();
            routeInspection.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void ActualRecipientsAreVisibleAndDoNotUseTheOldSwitchLabelAsAChoice()
        {
            Assert.That(routeInspection.ActualRecipients.ToArray(), Has.Length.EqualTo(2));
            Assert.That(routeInspection.ActualRecipients[0].RouteName, Is.EqualTo("Private headset"));
            Assert.That(routeInspection.ActualRecipients[0].Recipients, Is.EqualTo(new[] { "ada", "ren" }));
            Assert.That(routeInspection.ActualRecipients[1].RouteName, Is.EqualTo("Room speaker"));
            Assert.That(routeInspection.ActualRecipients[1].Recipients, Is.EqualTo(new[] { "ivo", "player", "ren" }));
        }

        [Test]
        public void BothReadableRecordsAreRequiredBeforeOneIdempotentPrivateRouteFact()
        {
            Assert.That(routeInspection.Open(), Is.True);
            Assert.That(routeInspection.VerifyPrivateRoute(), Is.EqualTo(RouteInspectionResult.Rejected));
            Assert.That(session.State.PrivateRouteVerified, Is.False);

            routeInspection.Tick(new RouteInspectionInputFrame(false, false, true, false));
            routeInspection.Tick(new RouteInspectionInputFrame(false, true, false, false));
            routeInspection.Tick(new RouteInspectionInputFrame(false, false, true, false));
            Assert.That(routeInspection.Model.DiagramRead, Is.True);
            Assert.That(routeInspection.Model.ConnectionRead, Is.True);
            Assert.That(routeInspection.VerifyPrivateRoute(), Is.EqualTo(RouteInspectionResult.Verified));
            Assert.That(session.State.PrivateRouteVerified, Is.True);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(session.State.Promises, Is.Empty);
            Assert.That(session.State.AcquiredSourceIds, Is.Empty);
            Assert.That(routeInspection.VerifyPrivateRoute(), Is.EqualTo(RouteInspectionResult.AlreadyVerified));
            Assert.That(routeInspection.LastActionMessage, Does.Contain("does not authorize"));
        }

        [Test]
        public void CancelLeavesBothRouteAndConsentUntouchedAndReleasesTheSharedModal()
        {
            Assert.That(routeInspection.Open(), Is.True);
            Assert.That(modalInput.IsOwner(routeInspection), Is.True);
            routeInspection.Tick(new RouteInspectionInputFrame(false, false, true, false));
            modalInput.HandleCancel();

            Assert.That(routeInspection.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(session.State.PrivateRouteVerified, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        [Test]
        public void WorldTargetProvidesKeyboardReachableRouteInspectionWithoutChangingConsent()
        {
            var routingSwitch = new GameObject("Routing Switch");
            try
            {
                var target = routingSwitch.AddComponent<RouteInspectionWorldTarget>();
                target.Configure(routeInspection);

                Assert.That(target.Prompt, Is.EqualTo("Inspect routing switch"));
                Assert.That(target.Interact(), Is.True);
                Assert.That(routeInspection.IsOpen, Is.True);
                Assert.That(session.State.PrivateRouteVerified, Is.False);
                Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            }
            finally
            {
                Object.DestroyImmediate(routingSwitch);
            }
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostProvidesAReachableRoutingSwitchThroughTheSharedFocusPath()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);

            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var inspection = Object.FindAnyObjectByType<InspectionController>();
            var route = Object.FindAnyObjectByType<RouteInspectionController>();
            var session = Object.FindAnyObjectByType<CaseSession>();
            var target = Object.FindAnyObjectByType<RouteInspectionWorldTarget>();
            Assert.That(opening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(route, Is.Not.Null);
            Assert.That(session, Is.Not.Null);
            Assert.That(target, Is.Not.Null);

            opening.Close();
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            Assert.That(capsule, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            capsule.enabled = false;
            inspection.transform.position = new Vector3(7.5f, 0f, 0.6f);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.localRotation = Quaternion.identity;
            capsule.enabled = true;
            Physics.SyncTransforms();

            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.FocusedPrompt, Is.EqualTo("[E] Inspect routing switch"));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(route.IsOpen, Is.True);
            Assert.That(session.State.PrivateRouteVerified, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
