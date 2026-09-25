using System.Collections;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Notebook;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class EvidenceComparisonTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private NotebookController notebook;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Evidence Comparison Test Player");
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = Vector3.up * 0.9f;
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            var content = CaseContent.LoadBaseline();
            session = player.AddComponent<CaseSession>();
            session.Configure(content, new CaseState(content));
            notebook = player.AddComponent<NotebookController>();
            notebook.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void ComparisonAllowsIdenticalSelectionAndReplacement()
        {
            Acquire("roster", "D01");
            Acquire("repair-note", "D02");
            OpenComparison();

            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D01", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.LeftSourceId, Is.EqualTo("D01"));
            Assert.That(notebook.Comparison.RightSourceId, Is.EqualTo("D01"));

            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D02", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.RightSourceId, Is.EqualTo("D02"));
        }

        [Test]
        public void ComparisonRejectsUnavailableEvidenceAndShowsMissingTimeExplicitly()
        {
            Acquire("roster", "D01");
            OpenComparison();

            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D02", notebook.Entries), Is.False);
            Assert.That(notebook.Comparison.LeftSourceId, Is.Null);
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.ResolveLeft(notebook.Entries).RecordedAt, Is.EqualTo("Time not recorded"));

            Acquire("tape", "T02");
            notebook.RefreshEntries();
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "T02", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.ResolveRight(notebook.Entries).RecordedAt, Is.EqualTo("Dated before the current call"));
        }

        [Test]
        public void ComparingCompositeEvidenceDoesNotConfirmIdentityOrGrantConsent()
        {
            Acquire("roster", "D01");
            Acquire("repair-note", "D02");
            var acquiredBefore = session.State.AcquiredSourceIds.ToArray();
            var identityBefore = session.State.IdentityConfirmed;
            var authorizationBefore = session.State.Authorization;
            OpenComparison();

            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D02", notebook.Entries), Is.True);

            Assert.That(notebook.Comparison.ResolveLeft(notebook.Entries).Evidence.Single(item => item.EvidenceId == "E03").IsComplete, Is.True);
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(acquiredBefore));
            Assert.That(session.State.IdentityConfirmed, Is.EqualTo(identityBefore));
            Assert.That(session.State.Authorization, Is.EqualTo(authorizationBefore));
        }

        [UnityTest]
        public IEnumerator KeyboardControlsSelectAndReplaceEitherSideWhileEscapeReturnsToNotebook()
        {
            Acquire("call", "T01");
            Acquire("tape", "T02");
            notebook.Tick(new NotebookInputFrame(true, false, false));
            notebook.Tick(new NotebookInputFrame(false, false, false, comparisonToggle: true));
            notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSetSelected: true));
            Assert.That(notebook.Comparison.LeftSourceId, Is.EqualTo("T01"));

            notebook.Tick(new NotebookInputFrame(false, true, false));
            notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSideToggle: true));
            notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSetSelected: true));
            Assert.That(notebook.Comparison.RightSourceId, Is.EqualTo("T02"));

            modalInput.HandleCancel();
            yield return null;
            Assert.That(notebook.Comparison.IsOpen, Is.False);
            Assert.That(notebook.IsOpen, Is.True);
            Assert.That(modalInput.IsOwner(notebook), Is.True);
        }

        private void Acquire(string recoveryId, string sourceId)
        {
            session.State.AcquireSource(recoveryId, sourceId);
            notebook.RefreshEntries();
        }

        private void OpenComparison()
        {
            Assert.That(notebook.TryOpen(), Is.True);
            Assert.That(notebook.Comparison.Open(notebook.Entries), Is.True);
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
