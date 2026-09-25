using System.Collections;
using System.Linq;
using CommonHouse.Interaction;
using CommonHouse.Opening;
using CommonHouse.Persistence;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class ControlGuidanceTests
    {
        [UnityTest]
        public IEnumerator GeneratedWelcomeAndPlainPauseExposeExistingControlsWithoutChangingCaseState()
        {
            var originalScale = CaseUi.TextScale;
            try
            {
                yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
                yield return null;
                var opening = Object.FindAnyObjectByType<OpeningHouseholdController>();
                var welcome = Object.FindAnyObjectByType<OpeningHouseholdOverlay>();
                var pause = Object.FindAnyObjectByType<InspectionOverlay>();
                var modal = Object.FindAnyObjectByType<ModalInputCoordinator>();
                var saves = Object.FindAnyObjectByType<CaseRuntimeSaveController>();
                var session = Object.FindAnyObjectByType<CaseSession>();
                Assert.That(opening, Is.Not.Null);
                Assert.That(welcome, Is.Not.Null);
                Assert.That(pause, Is.Not.Null);
                Assert.That(modal, Is.Not.Null);
                Assert.That(saves, Is.Not.Null);
                Assert.That(session, Is.Not.Null);

                var sourcesBefore = session.State.AcquiredSourceIds.ToArray();
                var authorizationBefore = session.State.Authorization;
                var outcomeBefore = session.State.OutcomeId;
                CaseUi.TextScale = CaseUi.MaximumTextScale;
                Assert.That(welcome.VisibleGuidanceText, Is.EqualTo(ControlsReference.WelcomeText));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[WASD] Move"));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[E] Use the focused object"));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[R] Review received transcripts"));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[H] Hint"));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[F5] Save"));
                Assert.That(welcome.VisibleGuidanceText, Does.Contain("[Escape] Pause"));

                opening.Close();
                Assert.That(modal.HasOwner, Is.False);
                Assert.That(modal.IsPaused, Is.False);
                Assert.That(saves.IsHudVisible, Is.True);
                modal.HandleCancel();
                Assert.That(modal.IsPaused, Is.True);
                Assert.That(pause.IsPauseGuidanceVisible, Is.True);
                Assert.That(pause.VisiblePauseGuidanceText, Is.EqualTo(ControlsReference.PauseText));
                Assert.That(pause.VisiblePauseGuidanceText, Does.Contain("[Escape] Resume"));
                Assert.That(saves.IsHudVisible, Is.False, "The save HUD must not cover the paused controls reference.");
                modal.HandleCancel();
                Assert.That(modal.IsPaused, Is.False);
                Assert.That(pause.IsPauseGuidanceVisible, Is.False);

                Assert.That(opening.OpenCare(), Is.True);
                Assert.That(modal.HasOwner, Is.True);
                modal.HandleCancel();
                Assert.That(opening.IsCarePanelOpen, Is.False, "Escape closes the active welcome panel before it can pause movement.");
                Assert.That(modal.HasOwner, Is.False);
                Assert.That(modal.IsPaused, Is.False);

                Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(sourcesBefore));
                Assert.That(session.State.Authorization, Is.EqualTo(authorizationBefore));
                Assert.That(session.State.OutcomeId, Is.EqualTo(outcomeBefore));
            }
            finally
            {
                CaseUi.TextScale = originalScale;
            }
        }
    }
}
