using System;
using System.Collections;
using System.Linq;
using CommonHouse.Domain;
using CommonHouse.Intro;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class IntroOpeningTests
    {
        private IntroOpeningController opening;
        private InspectionController inspection;
        private FirstPersonMovement movement;
        private ModalInputCoordinator modal;
        private Camera camera;
        private IntroOpeningTarget[] targets;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("IntroOpening", LoadSceneMode.Single);
            opening = Object.FindFirstObjectByType<IntroOpeningController>();
            inspection = Object.FindFirstObjectByType<InspectionController>();
            movement = Object.FindFirstObjectByType<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modal = movement.GetComponent<ModalInputCoordinator>();
            camera = movement.GetComponentInChildren<Camera>();
            targets = Object.FindObjectsByType<IntroOpeningTarget>(FindObjectsSortMode.None);
            CaseUi.TextScale = 1f;
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Intro test cleanup " + Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(scene);
            CaseUi.TextScale = 1f;
        }
        private IntroOpeningTarget Target(string name) => targets.Single(item => item.name == name);
        private void Focus(IntroOpeningTarget target)
        {
            var capsule = movement.GetComponent<CharacterController>();
            capsule.enabled = false;
            movement.transform.position = new Vector3(target.transform.position.x, .05f, target.transform.position.z - 1.5f);
            movement.transform.rotation = Quaternion.identity;
            camera.transform.LookAt(target.transform.position);
            capsule.enabled = true;
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
        }
        private void Open(string name)
        {
            Focus(Target(name));
            inspection.Tick(new InspectionInputFrame(true, false, false));
            Assert.That(opening.IsOpen, Is.True);
            Assert.That(modal.IsOwner(opening), Is.True);
            Assert.That(movement.MenuOpen, Is.True);
        }
        private void Act(IntroActionKind kind, string optionId = null, bool keyboard = true)
        {
            var index = opening.Actions.Select((item, i) => new { item, i })
                .Single(pair => pair.item.Kind == kind && (optionId == null || pair.item.OptionId == optionId)).i;
            if (keyboard)
            {
                var limit = opening.Actions.Count;
                while (opening.SelectedIndex != index && limit-- > 0) opening.Tick(false, true, false);
                Assert.That(opening.SelectedIndex, Is.EqualTo(index));
                opening.Tick(false, false, true);
            }
            else Assert.That(opening.Activate(index), Is.True);
        }

        [Test]
        public void SavedSceneContainsOnlyTheIsolatedOpeningAndReachableTargets()
        {
            Assert.That(targets.Length, Is.EqualTo(6));
            Assert.That(Object.FindFirstObjectByType<CaseSession>(), Is.Null);
            Assert.That(opening.State.AcquiredSourceIds, Is.Empty);
            Assert.That(opening.State.IsComplete, Is.False);
            Assert.That(opening.IsOpen, Is.False, "Arrival does not seize a panel automatically.");
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(Target("Entrance notice")), "The first interaction should be discoverable from the spawn view.");
            foreach (var target in targets)
            {
                Focus(target);
                Assert.That(inspection.FocusedPrompt, Does.StartWith("[E] "));
            }
            Assert.That(opening.OpenScene("IN-S04"), Is.False);
            Assert.That(opening.OpenScene("IN-S06"), Is.False);
            Assert.That(opening.OpenScene("IN-S07"), Is.False);
            Assert.That(opening.OpenSource("IN-T05"), Is.False);
            Assert.That(modal.HasOwner, Is.False);
        }

        [Test]
        public void KeyboardRouteOffersArrivalReceptionWarningAndShelterWithoutStartingACase()
        {
            Open("Entrance notice");
            Act(IntroActionKind.Acknowledge);
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q01-C");
            Assert.That(opening.State.ActiveReview.Lines.Select(line => line.Id), Does.Contain("IN-L014").And.Not.Contain("IN-L012"));
            Assert.That(opening.State.Expressions, Is.Empty);
            Act(IntroActionKind.Acknowledge);
            Assert.That(Target("Ren at threshold").ContentId, Is.EqualTo("IN-S02"));
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q04-A");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.State.WarningAcknowledged, Is.False, "One answer is not the whole warning account.");
            Open("Reception ledger");
            Act(IntroActionKind.Option, "IN-Q06-B");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.State.PreferredAddress, Is.Null);
            Open("Return warning");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.State.WarningAcknowledged, Is.True);
            Open("Food and bedding");
            Act(IntroActionKind.Option, "IN-Q02-C");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.State.ShelterAcknowledged, Is.True);
            Assert.That(opening.State.IsComplete, Is.False, "This slice is not the full intro.");
            Assert.That(opening.State.GreetingReviewed, Is.False);
            Assert.That(opening.State.NetworkAcknowledged, Is.False);
            Assert.That(modal.HasOwner, Is.False);
            Assert.That(movement.MenuOpen, Is.False);
        }

        [Test]
        public void EscapeDiscardsPendingChoiceAndReopeningRetainsCommittedProgress()
        {
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q01-D");
            modal.HandleCancel();
            Assert.That(opening.State.Expressions, Is.Empty);
            Assert.That(opening.State.AcknowledgedSceneIds, Is.Empty);
            Assert.That(opening.IsOpen, Is.False);
            Assert.That(movement.MenuOpen, Is.False);
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q01-D");
            Act(IntroActionKind.Acknowledge);
            var lines = opening.State.ReviewedLineIds.ToArray();
            Open("Reception ledger");
            Act(IntroActionKind.Option, "IN-Q06-A");
            Assert.That(opening.LastActionMessage, Does.Contain("later"));
            modal.HandleCancel();
            Assert.That(opening.State.PreferredAddress, Is.Null);
            Assert.That(opening.State.ReviewedLineIds, Is.EqualTo(lines));
            Assert.That(opening.State.Expressions.ContainsKey("IN-Q06"), Is.False);
            Assert.That(opening.ThresholdSceneId, Is.EqualTo("IN-S02"));
        }

        [Test]
        public void NeedAMinuteKeepsTheArrivalRecoverable()
        {
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q01-E");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.ThresholdSceneId, Is.EqualTo("IN-S01"));
            Open("Ren at threshold");
            Act(IntroActionKind.Option, "IN-Q01-D");
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.ThresholdSceneId, Is.EqualTo("IN-S02"));
        }

        [Test]
        public void PlainPauseListsOnlyAvailableIntroControlsAndResumesWithoutLearning()
        {
            var overlay = opening.GetComponent<IntroOpeningOverlay>();
            modal.HandleCancel();
            Assert.That(overlay.IsPauseGuidanceVisible, Is.True);
            Assert.That(overlay.VisiblePauseGuidanceText, Does.Contain("[Escape]").And.Not.Contain("Save").And.Not.Contain("Notebook").And.Not.Contain("Hint"));
            Assert.That(opening.State.ReviewedLineIds, Is.Empty);
            modal.HandleCancel();
            Assert.That(overlay.IsPauseGuidanceVisible, Is.False);
            Assert.That(movement.MenuOpen, Is.False);
        }

        [Test]
        public void PauseAndReadableSkipDoNotAcknowledgeOrMoveTheCamera()
        {
            Open("Food and bedding");
            var position = movement.transform.position;
            var rotation = camera.transform.rotation;
            Act(IntroActionKind.Pause);
            Assert.That(opening.State.IsPaused, Is.True);
            Assert.That(opening.Actions.Any(action => action.Kind == IntroActionKind.Acknowledge), Is.False);
            Act(IntroActionKind.Transcript);
            Assert.That(opening.State.TranscriptOnly, Is.True);
            Assert.That(opening.State.IsPaused, Is.True);
            movement.Tick(new FirstPersonInputFrame(Vector2.up, Vector2.one), .1f);
            Assert.That(movement.transform.position, Is.EqualTo(position));
            Assert.That(camera.transform.rotation, Is.EqualTo(rotation));
            Assert.That(opening.State.ShelterAcknowledged, Is.False);
            Act(IntroActionKind.Resume);
            Act(IntroActionKind.Acknowledge);
            Assert.That(opening.State.ShelterAcknowledged, Is.True);
            Assert.That(movement.transform.position, Is.EqualTo(position));
            Assert.That(camera.transform.rotation, Is.EqualTo(rotation));
        }

        [Test]
        public void CompetingModalOwnershipCannotOpenOrAcquireIntroContent()
        {
            var other = new OtherPanel();
            Assert.That(modal.TryOpen(other), Is.True);
            Assert.That(Target("Return warning").Interact(), Is.False);
            Assert.That(opening.State.ActiveReview, Is.Null);
            Assert.That(opening.State.WarningAcknowledged, Is.False);
            Assert.That(modal.IsOwner(other), Is.True);
            modal.Release(other);
            Open("Return warning");
            Assert.That(opening.VisibleTitle, Is.EqualTo("Ren's return warning"));
            Assert.That(opening.OpenScene("IN-S03"), Is.False);
            Assert.That(opening.State.ActiveReview.Id, Is.EqualTo("IN-C01"));
            Assert.That(opening.State.AcquiredSourceIds, Is.Empty);
        }

        [Test]
        public void WorldFocusHonorsRangeAndOcclusion()
        {
            var target = Target("Ren at threshold");
            Focus(target);
            var capsule = movement.GetComponent<CharacterController>();
            capsule.enabled = false;
            movement.transform.position -= Vector3.forward * 2f;
            camera.transform.LookAt(target.transform.position);
            capsule.enabled = true;
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.Null);
            Assert.That(inspection.TryActivateFocused(), Is.False);
            Focus(target);
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                blocker.transform.position = Vector3.Lerp(camera.transform.position, target.transform.position, .5f);
                blocker.transform.localScale = new Vector3(.8f, .8f, .1f);
                Physics.SyncTransforms();
                Assert.That(inspection.TryActivateFocused(), Is.False);
                Assert.That(opening.IsOpen, Is.False);
                Assert.That(opening.State.ReviewedLineIds, Is.Empty);
            }
            finally { Object.DestroyImmediate(blocker); }
        }

        [Test]
        public void MouseAndKeyboardUseTheSameShelterActionsAndReplayIsStable()
        {
            foreach (var option in new[] { "IN-Q02-A", "IN-Q02-B", "IN-Q02-C" })
            {
                Open("Food and bedding");
                Act(IntroActionKind.Option, option, false);
                Act(IntroActionKind.Acknowledge, keyboard: false);
                Assert.That(opening.State.ShelterAcknowledged, Is.True);
                Assert.That(opening.State.Expressions["IN-Q02"], Is.EqualTo(option));
            }
            Open("Return warning");
            Act(IntroActionKind.Acknowledge);
            var sources = opening.State.AcquiredSourceIds.ToArray();
            var lines = opening.State.ReviewedLineIds.ToArray();
            Open("Return warning");
            Act(IntroActionKind.Acknowledge, keyboard: false);
            Assert.That(opening.State.AcquiredSourceIds, Is.EqualTo(sources));
            Assert.That(opening.State.ReviewedLineIds, Is.EqualTo(lines));
            Assert.That(opening.State.IsComplete, Is.False);
        }

        [UnityTest]
        public IEnumerator PlayerCanWalkFromThresholdThroughDoorwayToWelcomeArea()
        {
            foreach (var point in new[] { new Vector3(1f, 0f, -.7f), new Vector3(.8f, 0f, .6f),
                new Vector3(0f, 0f, .8f), new Vector3(0f, 0f, 3f), new Vector3(-1.5f, 0f, 4f) })
            {
                for (var frame = 0; frame < 150; frame++)
                {
                    var delta = point - movement.transform.position;
                    var flat = new Vector2(delta.x, delta.z);
                    if (flat.magnitude < .16f) break;
                    movement.Tick(new FirstPersonInputFrame(flat.normalized, Vector2.zero), .04f);
                    yield return null;
                }
                Assert.That(Vector2.Distance(new Vector2(movement.transform.position.x, movement.transform.position.z), new Vector2(point.x, point.z)), Is.LessThan(.2f));
            }
            Assert.That(opening.State.AcquiredSourceIds, Is.Empty, "Walking past residents does not acquire dialogue.");
        }

        private sealed class FixedInput : IFirstPersonInput { public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero); }
        private sealed class OtherPanel : IModalPanel { public void CloseFromModalInput() { } }
    }
}
