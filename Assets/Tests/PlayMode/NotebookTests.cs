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
    public class NotebookTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private NotebookController notebook;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Notebook Test Player");
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = Vector3.up * 0.9f;
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            session.Configure(CaseContent.LoadBaseline(), new CaseState(CaseContent.LoadBaseline()));
            notebook = player.AddComponent<NotebookController>();
            notebook.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void NotebookShowsOnlyAcquiredOriginalSourceAndItsKnownClaimLabel()
        {
            session.State.AcquireSource("call", "T01");
            notebook.RefreshEntries();

            Assert.That(notebook.Entries.Select(item => item.SourceId), Is.EqualTo(new[] { "T01" }));
            var entry = notebook.SelectedEntry;
            Assert.That(entry.OriginalText, Does.Contain("Please tell Mara"));
            Assert.That(entry.OriginalText, Does.Not.Contain("shared repair"));
            var claim = entry.Evidence.Single(item => item.EvidenceId == "E01").Statements.Single();
            Assert.That(claim.Label, Is.EqualTo("Attributed claim"));
            Assert.That(claim.Text, Does.Contain("neither identity is established"));
        }

        [Test]
        public void CompositeEvidenceKeepsAnAcquiredSourceWithoutRevealingMissingSourceText()
        {
            session.State.AcquireSource("roster", "D01");
            notebook.RefreshEntries();

            var partial = notebook.SelectedEntry.Evidence.Single(item => item.EvidenceId == "E03");
            Assert.That(partial.AcquiredSources, Is.EqualTo(1));
            Assert.That(partial.TotalSources, Is.EqualTo(2));
            Assert.That(partial.IsComplete, Is.False);
            Assert.That(partial.Statements, Is.Empty);
            Assert.That(notebook.SelectedEntry.OriginalText, Does.Contain("Old roster"));

            session.State.AcquireSource("repair-note", "D02");
            notebook.RefreshEntries();
            var complete = notebook.Entries.Single(item => item.SourceId == "D01").Evidence.Single(item => item.EvidenceId == "E03");
            Assert.That(complete.IsComplete, Is.True);
            Assert.That(complete.Statements.Select(item => item.Label), Is.EquivalentTo(new[] { "Observation", "Hypothesis" }));
        }

        [Test]
        public void NotebookUsesSharedSessionAfterRevisitingAndModalCancelReturnsFocus()
        {
            session.State.AcquireSource("private-question", "C01");
            notebook.Tick(new NotebookInputFrame(true, false, false));
            Assert.That(notebook.IsOpen, Is.True);
            Assert.That(modalInput.IsOwner(notebook), Is.True);

            modalInput.HandleCancel();
            Assert.That(notebook.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);

            var revisitedNotebook = player.AddComponent<NotebookController>();
            revisitedNotebook.Configure(session, modalInput);
            Assert.That(revisitedNotebook.TryOpen(), Is.True);
            Assert.That(revisitedNotebook.Entries.Select(item => item.SourceId), Is.EqualTo(new[] { "C01" }));
            Assert.That(revisitedNotebook.SelectedEntry.Evidence.Single().Statements.Single().Label, Is.EqualTo("Attributed claim"));
        }

        [Test]
        public void KeyboardSelectionCyclesAcquiredRecordsWithoutChangingCaseState()
        {
            session.State.AcquireSource("call", "T01");
            session.State.AcquireSource("tape", "T02");
            notebook.Tick(new NotebookInputFrame(true, false, false));
            var initiallySelected = notebook.SelectedEntry.SourceId;
            notebook.Tick(new NotebookInputFrame(false, true, false));

            Assert.That(notebook.SelectedEntry.SourceId, Is.Not.EqualTo(initiallySelected));
            notebook.Tick(new NotebookInputFrame(false, false, true));
            Assert.That(notebook.SelectedEntry.SourceId, Is.EqualTo(initiallySelected));
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(new[] { "T01", "T02" }));
        }

        [UnityTest]
        public IEnumerator NotebookNeverClaimsOwnershipWhenAnotherModalIsOpen()
        {
            var other = new TestModal();
            Assert.That(modalInput.TryOpen(other), Is.True);
            yield return null;
            Assert.That(notebook.TryOpen(), Is.False);
            Assert.That(notebook.IsOpen, Is.False);
            Assert.That(modalInput.IsOwner(other), Is.True);
            modalInput.Release(other);
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }

        private sealed class TestModal : IModalPanel
        {
            public void CloseFromModalInput() { }
        }
    }
}
