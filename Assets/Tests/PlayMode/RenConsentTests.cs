using System.Collections;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class RenConsentTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private DialoguePresenter presenter;
        private ResidentDialogueTarget ren;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Ren Consent Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            var content = CaseContent.LoadBaseline();
            session.Configure(content, new CaseState(content));
            presenter = player.AddComponent<DialoguePresenter>();
            presenter.Configure(session, modalInput);
            ren = new GameObject("Ren Private Alcove").AddComponent<ResidentDialogueTarget>();
            ren.Configure(presenter, "ren");
        }

        [TearDown]
        public void TearDown()
        {
            if (ren != null) Object.DestroyImmediate(ren.gameObject);
            if (player != null) Object.DestroyImmediate(player);
        }

        [Test]
        public void CurrentCallAlonePermitsThePrivateIdentityQuestionAndCanonicalRefusal()
        {
            session.State.AcquireSource("opening-call", "T01");

            Assert.That(ren.Prompt, Is.EqualTo("Speak privately with Ren"));
            Assert.That(ren.Interact(), Is.True);
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Does.Contain("ren-current-call"));
            Assert.That(session.State.HasEvidence("E03"), Is.False);

            Select(presenter, "ren-current-call");

            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ren"));
            Assert.That(presenter.CurrentLine.Text, Is.EqualTo("Ren: Mara was my network name. My location was relayed without my permission. I do not want immediate contact; you may relay my refusal."));
            Assert.That(session.State.HasEvidence("E04"), Is.True);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
        }

        [Test]
        public void InvestigationPrerequisitesDoNotGrantConsentAndLaterAssuranceDoes()
        {
            session.State.AcquireSource("opening-call", "T01");
            session.State.AcquireSource("ada-repair-account", "T03");
            session.State.AcquireSource("ren-repair-account", "C02");
            session.State.InspectPrivateRoute("route-check");
            Assert.That(session.State.HasEvidence("E06"), Is.True);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));

            ren.Interact();
            Select(presenter, "ren-current-call");
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
            presenter.Close();

            Assert.That(ren.Interact(), Is.True);
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Does.Contain("ren-private-contact"));
            Select(presenter, "ren-private-contact");
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
            Assert.That(presenter.CurrentLine.Text, Does.Contain("agree to speak privately"));
        }

        [Test]
        public void RepeatIdentityQuestionUsesItsStableEventsAndCannotDowngradeAnAgreement()
        {
            session.State.AcquireSource("opening-call", "T01");
            session.State.AcquireSource("private-account", "C01");
            session.State.RecordRefusalAuthorization("initial-refusal");
            session.State.AcquireSource("ada-repair-account", "T03");
            session.State.AcquireSource("ren-repair-account", "C02");
            session.State.InspectPrivateRoute("route-check");
            session.State.RecordPrivateContactAgreement("later-agreement");

            var repeat = new DialogueNode("ren-current-call", "ren", "Ask again.", "",
                new[] { DialogueEffectKind.AcquirePrivateAccount, DialogueEffectKind.AuthorizeRefusal },
                state => state.OutcomeId == null, "C01");
            Assert.That(presenter.Open(new[] { repeat }), Is.True);
            Assert.That(presenter.SelectCurrent(), Is.True);

            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
            Assert.That(session.State.AuthorizationEventId, Is.EqualTo("later-agreement"));
            Assert.That(presenter.CurrentLine.Text, Does.Contain("you may relay my refusal"));
        }

        [Test]
        public void CancellationAndLeaveOpenKeepThePrivateLeadAvailable()
        {
            session.State.AcquireSource("opening-call", "T01");
            Assert.That(ren.Interact(), Is.True);
            modalInput.HandleCancel();

            Assert.That(presenter.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(session.State.HasEvidence("E04"), Is.False);
            Assert.That(ren.Interact(), Is.True);
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Does.Contain("ren-current-call"));
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostReachesRenOnlyThroughThePrivateArchiveFocusTarget()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;

            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var inspection = Object.FindAnyObjectByType<InspectionController>();
            var dialogue = Object.FindAnyObjectByType<DialoguePresenter>();
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var residents = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var sceneRen = residents.SingleOrDefault(target => target.ResidentId == "ren");
            Assert.That(opening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(dialogue, Is.Not.Null);
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(sceneRen, Is.Not.Null);

            opening.Close();
            Focus(inspection, sceneRen.transform.position);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(sceneRen));
            Assert.That(inspection.FocusedPrompt, Is.EqualTo("[E] Speak privately with Ren"));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(dialogue.IsOpen, Is.True);
            Assert.That(sceneSession.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        private static void Select(DialoguePresenter dialogue, string nodeId)
        {
            var index = dialogue.EligibleQuestions.ToList().FindIndex(node => node.Id == nodeId);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "Expected authored node: " + nodeId);
            while (dialogue.SelectedIndex != index)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(dialogue.SelectCurrent(), Is.True);
        }

        private static void Focus(InspectionController inspection, Vector3 targetPosition)
        {
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            Assert.That(capsule, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            capsule.enabled = false;
            inspection.transform.position = targetPosition + new Vector3(0f, -1.4f, -1.9f);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.LookAt(targetPosition);
            capsule.enabled = true;
            Physics.SyncTransforms();
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
