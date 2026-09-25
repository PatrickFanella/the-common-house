using System.Collections.Generic;
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
    public class DialoguePresenterTests
    {
        private GameObject player;
        private ModalInputCoordinator modalInput;
        private DialoguePresenter presenter;
        private CaseState state;

        [SetUp]
        public void SetUp()
        {
            state = new CaseState(CaseContent.LoadBaseline());
            state.AcquireSource("opening-call", "T01");
            player = new GameObject("Dialogue Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            presenter = player.AddComponent<DialoguePresenter>();
            presenter.Configure(state, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void OnlyEligibleQuestionsAreShownAndThePrivateAnswerKeepsRenAttribution()
        {
            Assert.That(presenter.Open(), Is.True);
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Is.EquivalentTo(new[] { "ren-current-call", "ren-leave-open" }));
            Assert.That(presenter.SelectCurrent(), Is.True);
            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ren"));
            Assert.That(presenter.CurrentLine.Text, Is.EqualTo("Ren: Mara was my network name. My location was relayed without my permission. I do not want immediate contact; you may relay my refusal."));
            Assert.That(state.HasEvidence("E04"), Is.True);
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Does.Not.Contain("ren-current-call"));
        }

        [Test]
        public void CancellationRetainsRecoveryLeadAndCoordinatorOwnsClose()
        {
            var before = presenter.EligibleQuestions.Select(node => node.Id).ToArray();
            presenter.Open();
            modalInput.HandleCancel();

            Assert.That(presenter.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "T01" }));
            Assert.That(presenter.EligibleQuestions.Select(node => node.Id), Is.EqualTo(before));
        }

        [Test]
        public void RepeatedNodeDeliveryUsesFixedEventsAndDoesNotDuplicateDisclosuresOrPromises()
        {
            var repeated = new DialogueNode(
                "repeat-private-question", "ren", "Ask again.", "The same answer remains.",
                new[] { DialogueEffectKind.AcquirePrivateAccount, DialogueEffectKind.AuthorizeRefusal, DialogueEffectKind.ProtectPrivateContact },
                ignored => true);
            presenter.Configure(state, modalInput, new[] { repeated });
            presenter.Open();
            presenter.SelectCurrent();
            var disclosures = state.Disclosures.Count;
            var promises = state.Promises.Count;
            presenter.SelectCurrent();

            Assert.That(state.Disclosures.Count, Is.EqualTo(disclosures));
            Assert.That(state.Promises.Count, Is.EqualTo(promises));
            Assert.That(state.AuthorizationEventId, Is.EqualTo("dialogue:repeat-private-question:refusal"));
            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ren"));
        }

        [Test]
        public void KeyboardFrameNavigationHasNoTimerAndCanReachThePrivateAgreementQuestion()
        {
            state.AcquireSource("private-account", "C01");
            state.RecordRefusalAuthorization("initial-refusal");
            state.AcquireSource("ada-account", "T03");
            state.AcquireSource("ren-account", "C02");
            state.InspectPrivateRoute("checked-private-route");
            presenter.Open();
            var questions = presenter.EligibleQuestions;
            var privateIndex = questions.ToList().FindIndex(node => node.Id == "ren-private-contact");
            Assert.That(privateIndex, Is.GreaterThanOrEqualTo(0));
            while (presenter.SelectedIndex != privateIndex)
                presenter.Tick(new DialogueInputFrame(false, true, false));
            presenter.Tick(new DialogueInputFrame(false, false, true));

            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
            Assert.That(presenter.CurrentLine.SpeakerId, Is.EqualTo("ren"));
            Assert.That(presenter.CurrentLine.Text, Does.Contain("privately"));
        }

        [UnityTest]
        public System.Collections.IEnumerator GeneratedOutpostUsesEffectFreeDemoDialogue()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var scenePresenter = Object.FindAnyObjectByType<DialoguePresenter>();
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(scenePresenter, Is.Not.Null);
            Assert.That(scenePresenter.EligibleQuestions.Select(node => node.Id), Is.EquivalentTo(new[] { "outpost-check-in", "outpost-leave" }));

            Assert.That(scenePresenter.Open(), Is.True);
            Assert.That(scenePresenter.SelectCurrent(), Is.True);
            Assert.That(sceneSession.State.AcquiredSourceIds, Is.Empty);
            Assert.That(sceneSession.State.Disclosures, Is.Empty);
            Assert.That(sceneSession.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
