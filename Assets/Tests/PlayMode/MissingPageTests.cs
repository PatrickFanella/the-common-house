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
    public class MissingPageTests
    {
        private GameObject player;
        private ModalInputCoordinator modalInput;
        private DialoguePresenter presenter;
        private CaseState state;

        [SetUp]
        public void SetUp()
        {
            state = new CaseState(CaseContent.LoadBaseline());
            player = new GameObject("Missing Page Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            presenter = player.AddComponent<DialoguePresenter>();
            presenter.Configure(state, modalInput, IvoDialogueNodes.All);
        }

        [TearDown]
        public void TearDown()
        {
            if (player != null) Object.DestroyImmediate(player);
        }

        [Test]
        public void AskingIvoSuppliesCanonicalPageWithIvoAttributionAndE05()
        {
            Assert.That(state.AcquiredSourceIds, Is.Empty);
            Assert.That(QuestionIds(), Does.Contain("ivo-log-gap-request"));

            Select("ivo-log-gap-request");

            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "D03" }));
            Assert.That(state.HasEvidence("E05"), Is.True);
            Assert.That(state.Disclosures, Is.Empty, "D03 remains a document rather than a newly inferred conversation.");
            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ivo"));
            Assert.That(presenter.CurrentLine.Text, Does.Contain("unauthorized relay"));
            Assert.That(QuestionIds(), Does.Not.Contain("ivo-log-gap-request"));
            Assert.That(QuestionIds(), Does.Contain("ivo-log-gap-replay"));
        }

        [Test]
        public void DeclineCancellationAndRevisitKeepTheRequestAvailableWithoutTrespass()
        {
            Select("ivo-log-gap-decline");
            Assert.That(state.AcquiredSourceIds, Is.Empty);
            Assert.That(QuestionIds(), Does.Contain("ivo-log-gap-request"));

            modalInput.HandleCancel();
            Assert.That(presenter.IsOpen, Is.False);
            Assert.That(state.AcquiredSourceIds, Is.Empty);
            Assert.That(presenter.Open(), Is.True);
            Assert.That(QuestionIds(), Does.Contain("ivo-log-gap-request"));
            Assert.That(QuestionIds(), Does.Not.Contain("trespass"));
        }

        [Test]
        public void RepeatingTheSuppliedPageOnlyReplaysTheCanonicalSource()
        {
            Select("ivo-log-gap-request");
            var acquisitions = state.AcquiredSourceIds.ToArray();
            var disclosures = state.Disclosures.Count;
            presenter.Close();
            Assert.That(presenter.Open(), Is.True);
            Select("ivo-log-gap-replay");

            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ivo"));
            Assert.That(presenter.CurrentLine.Text, Does.Contain("Ivo’s involvement"));
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(acquisitions));
            Assert.That(state.Disclosures, Has.Count.EqualTo(disclosures));
        }

        [Test]
        public void IvoWorldTargetOpensOnlyTheIvoCatalogAndKeepsTheRecoveryPathReachable()
        {
            var resident = new GameObject("Ivo Dialogue Target");
            try
            {
                var target = resident.AddComponent<ResidentDialogueTarget>();
                target.Configure(presenter, "ivo");

                Assert.That(target.Prompt, Is.EqualTo("Speak with Ivo"));
                Assert.That(target.Interact(), Is.True);
                Assert.That(QuestionIds(), Does.Contain("ivo-log-gap-request"));
                Assert.That(QuestionIds(), Does.Not.Contain("ren-current-call"));
                Assert.That(state.AcquiredSourceIds, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(resident);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator GeneratedOutpostIvoPathSuppliesThePageWithoutDisclosureOrConsent()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null; // Allow the initial welcome Start callback before dismissing it.
            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var inspection = Object.FindAnyObjectByType<InspectionController>();
            var target = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Single(item => item.ResidentId == "ivo");
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var sceneDialogue = Object.FindAnyObjectByType<DialoguePresenter>();
            Assert.That(opening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(sceneDialogue, Is.Not.Null);
            opening.Close();

            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            capsule.enabled = false;
            inspection.transform.position = new Vector3(0f, 0f, 0.1f);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.localRotation = Quaternion.identity;
            capsule.enabled = true;
            Physics.SyncTransforms();

            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            var requestIndex = sceneDialogue.EligibleQuestions.ToList().FindIndex(node => node.Id == "ivo-log-gap-request");
            Assert.That(requestIndex, Is.GreaterThanOrEqualTo(0));
            while (sceneDialogue.SelectedIndex != requestIndex)
                sceneDialogue.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(sceneDialogue.SelectCurrent(), Is.True);

            Assert.That(sceneDialogue.CurrentLine.SpeakerId, Is.EqualTo("ivo"));
            Assert.That(sceneSession.State.AcquiredSourceIds, Is.EqualTo(new[] { "D03" }));
            Assert.That(sceneSession.State.HasEvidence("E05"), Is.True);
            Assert.That(sceneSession.State.Disclosures, Is.Empty);
            Assert.That(sceneSession.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        private string[] QuestionIds() => presenter.EligibleQuestions.Select(node => node.Id).ToArray();

        private void Select(string nodeId)
        {
            Assert.That(presenter.Open(), Is.True);
            var index = presenter.EligibleQuestions.ToList().FindIndex(node => node.Id == nodeId);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "Expected authored Ivo dialogue node.");
            while (presenter.SelectedIndex != index)
                presenter.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(presenter.SelectCurrent(), Is.True);
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
