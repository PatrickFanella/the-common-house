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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class RosterEvidenceTests
    {
        private GameObject player;
        private GameObject rosterObject;
        private GameObject repairNoteObject;
        private CaseSession session;
        private InspectionController inspection;
        private NotebookController notebook;
        private InspectionTarget roster;
        private InspectionTarget repairNote;
        private readonly Vector3 testOrigin = new Vector3(100f, 0f, 100f);

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Roster Evidence Test Player");
            player.transform.position = testOrigin;
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = Vector3.up * 0.9f;
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            var camera = new GameObject("Roster Evidence Test Camera").AddComponent<Camera>();
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = Vector3.up * 1.6f;
            movement.SetViewTransform(camera.transform);
            var modalInput = player.AddComponent<ModalInputCoordinator>();
            inspection = player.AddComponent<InspectionController>();
            inspection.Configure(camera, movement, modalInput, 2.5f);
            var content = CaseContent.LoadBaseline();
            session = player.AddComponent<CaseSession>();
            session.Configure(content, new CaseState(content));
            var binding = player.AddComponent<CaseInspectionBinding>();
            binding.Configure(session, inspection);
            binding.Bind();
            notebook = player.AddComponent<NotebookController>();
            notebook.Configure(session, modalInput);
            roster = CreateDocumentTarget("Old Mara roster", "D01", out rosterObject);
            repairNote = CreateDocumentTarget("Ren repair note", "D02", out repairNoteObject);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rosterObject);
            Object.DestroyImmediate(repairNoteObject);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void ExplicitInspectionMakesBothCanonicalDocumentsComparableWithoutConfirmingIdentity()
        {
            Assert.That(Acquire(roster), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(new[] { "D01" }));
            Assert.That(session.State.IdentityConfirmed, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));

            Assert.That(Acquire(repairNote), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            notebook.RefreshEntries();
            Assert.That(notebook.Entries.Select(item => item.SourceId), Is.EqualTo(new[] { "D01", "D02" }));
            Assert.That(notebook.Entries.Single(item => item.SourceId == "D01").OriginalText,
                Is.EqualTo(session.Content.Sources.Single(item => item.Id == "D01").Text));
            Assert.That(notebook.Entries.Single(item => item.SourceId == "D02").OriginalText,
                Is.EqualTo(session.Content.Sources.Single(item => item.Id == "D02").Text));
            Assert.That(notebook.Entries.Single(item => item.SourceId == "D01").Evidence.Single(item => item.EvidenceId == "E03").IsComplete, Is.True);

            Assert.That(notebook.TryOpen(), Is.True);
            Assert.That(notebook.Comparison.Open(notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", notebook.Entries), Is.True);
            Assert.That(notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D02", notebook.Entries), Is.True);
            Assert.That(session.State.IdentityConfirmed, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        [Test]
        public void RepeatedEnterIsIdempotentAndUnknownOrNonDocumentSourcesAreRejected()
        {
            Assert.That(Acquire(roster), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            inspection.RefreshFocus();
            Assert.That(inspection.TryOpenFocused(), Is.True);
            Assert.That(inspection.TryAcquireOpen(), Is.EqualTo(InspectionAcquisitionResult.AlreadyAcquired));
            inspection.Close();

            var ledger = new CaseInspectionLedger(session);
            Assert.That(ledger.TryAcquire("missing-document"), Is.EqualTo(InspectionAcquisitionResult.Rejected));
            Assert.That(ledger.TryAcquire("T01"), Is.EqualTo(InspectionAcquisitionResult.Rejected));
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(new[] { "D01" }));
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostInspectsBothDocumentsBeforeComparisonWithoutChangingAuthorization()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;

            var opening = Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            Assert.That(opening, Is.Not.Null);
            opening.Close();
            var sceneSession = Object.FindFirstObjectByType<CaseSession>();
            var sceneInspection = Object.FindFirstObjectByType<InspectionController>();
            var sceneNotebook = Object.FindFirstObjectByType<NotebookController>();
            var camera = Camera.main;
            var sceneTargets = Object.FindObjectsByType<InspectionTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var sceneD01 = sceneTargets.Single(item => item.AcquisitionId == "D01");
            var sceneD02 = sceneTargets.Single(item => item.AcquisitionId == "D02");
            Assert.That(camera, Is.Not.Null);

            InspectGeneratedTarget(camera, sceneInspection, sceneD02, sceneSession);
            InspectGeneratedTarget(camera, sceneInspection, sceneD01, sceneSession);
            Assert.That(sceneSession.State.AcquiredSourceIds, Is.EqualTo(new[] { "D01", "D02" }));

            sceneNotebook.RefreshEntries();
            Assert.That(sceneNotebook.Entries.Single(item => item.SourceId == "D01").Evidence.Single(item => item.EvidenceId == "E03").IsComplete, Is.True);
            Assert.That(sceneNotebook.TryOpen(), Is.True);
            Assert.That(sceneNotebook.Comparison.Open(sceneNotebook.Entries), Is.True);
            Assert.That(sceneNotebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", sceneNotebook.Entries), Is.True);
            Assert.That(sceneNotebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D02", sceneNotebook.Entries), Is.True);
            Assert.That(sceneSession.State.IdentityConfirmed, Is.False);
            Assert.That(sceneSession.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        private InspectionAcquisitionResult Acquire(InspectionTarget target)
        {
            rosterObject.transform.position = testOrigin + (target == roster ? new Vector3(0f, 1.6f, 2f) : new Vector3(0f, 1.6f, 8f));
            repairNoteObject.transform.position = testOrigin + (target == repairNote ? new Vector3(0f, 1.6f, 2f) : new Vector3(0f, 1.6f, 8f));
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentFocus, Is.EqualTo(target));
            Assert.That(inspection.TryOpenFocused(), Is.True);
            var result = inspection.TryAcquireOpen();
            inspection.Close();
            return result;
        }

        private static void InspectGeneratedTarget(Camera camera, InspectionController controller, InspectionTarget target, CaseSession sceneSession)
        {
            camera.transform.position = target.transform.position + new Vector3(0f, 0f, -0.25f);
            camera.transform.LookAt(target.transform.position);
            Physics.SyncTransforms();
            controller.RefreshFocus();
            Assert.That(controller.CurrentFocus, Is.EqualTo(target));
            Assert.That(controller.TryOpenFocused(), Is.True);
            Assert.That(controller.VisiblePanelText, Is.EqualTo(sceneSession.Content.Sources.Single(item => item.Id == target.AcquisitionId).Text));
            Assert.That(sceneSession.State.AcquiredSourceIds, Does.Not.Contain(target.AcquisitionId));
            Assert.That(controller.TryAcquireOpen(), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            controller.Close();
        }

        private InspectionTarget CreateDocumentTarget(string title, string sourceId, out GameObject targetObject)
        {
            var source = session.Content.Sources.Single(item => item.Id == sourceId);
            targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetObject.name = title;
            targetObject.transform.localScale = new Vector3(1f, 1f, 0.1f);
            var target = targetObject.AddComponent<InspectionTarget>();
            target.Configure("case-" + source.Id.ToLowerInvariant(), title, source.Text, source.Id);
            return target;
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }

}
