using System;
using System.Collections;
using System.IO;
using System.Linq;
using CommonHouse.Dialogue;
using CommonHouse.Interaction;
using CommonHouse.Opening;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class ResidentConversationFacingTests
    {
        private ResidentMotion[] residents;
        private DialoguePresenter dialogue;
        private ModalInputCoordinator modal;
        private Camera view;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<OpeningHouseholdController>().Close();
            residents = Object.FindObjectsByType<ResidentMotion>(FindObjectsSortMode.None);
            Assert.That(residents, Has.Length.EqualTo(2));
            dialogue = Object.FindAnyObjectByType<DialoguePresenter>();
            modal = Object.FindAnyObjectByType<ModalInputCoordinator>();
            view = modal.GetComponentInChildren<Camera>();
            Object.FindAnyObjectByType<CaseSession>().State.AcquireSource("facing-test-call", "T01");
        }

        [Test]
        public void StandingResidentTurnsGraduallyWhileListeningWithoutMovingOrGrantingEvidence()
        {
            var ren = Resident("ren");
            var position = ren.transform.position;
            var targetPosition = Target("ren").transform.position;
            var initial = ren.transform.rotation;
            view.transform.position = position + Vector3.right * 2f + Vector3.up * 1.6f;
            Assert.That(Target("ren").Interact(), Is.True);
            Assert.That(dialogue.CurrentLine, Is.Null);
            Facing(ren, 0.1f);
            Assert.That(Quaternion.Angle(initial, ren.transform.rotation), Is.InRange(1f, 12.1f));
            Facing(ren, 2f);
            Assert.That(Vector3.Dot(ren.transform.forward, Vector3.right), Is.GreaterThan(0.99f));
            Assert.That(ren.IsFacingPlayer, Is.True);
            Assert.That(ren.transform.position, Is.EqualTo(position));
            Assert.That(Target("ren").transform.position, Is.EqualTo(targetPosition));
            Assert.That(Object.FindAnyObjectByType<CaseSession>().State.HasEvidence("E04"), Is.False);
        }

        [Test]
        public void InactiveResidentsDoNotTrackThePlayer()
        {
            var ivo = Resident("ivo");
            var ren = Resident("ren");
            var ivoRotation = ivo.transform.rotation;
            var renRotation = ren.transform.rotation;
            view.transform.position = ivo.transform.position + Vector3.right * 2f + Vector3.up * 1.6f;
            Facing(ivo, 2f); Facing(ren, 2f);
            Assert.That(Quaternion.Angle(ivoRotation, ivo.transform.rotation), Is.LessThan(0.1f));
            Assert.That(Quaternion.Angle(renRotation, ren.transform.rotation), Is.LessThan(0.1f));
            Assert.That(Target("ivo").Interact(), Is.True);
            Facing(ivo, 2f); Facing(ren, 2f);
            Assert.That(ivo.IsFacingPlayer, Is.True);
            Assert.That(ren.IsFacingPlayer, Is.False);
            Assert.That(Quaternion.Angle(renRotation, ren.transform.rotation), Is.LessThan(0.1f));
        }

        [Test]
        public void SeatedResidentKeepsChairAlignmentAndLimitsActualHeadYawAndPitch()
        {
            var ren = Resident("ren");
            var head = ren.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            var reference = Quaternion.Inverse(ren.transform.rotation) * head.rotation;
            Seat(ren);
            var position = ren.transform.position;
            var body = ren.transform.rotation;
            var targetPosition = Target("ren").transform.position;
            view.transform.position = head.position + ren.transform.right * 2f + Vector3.up * 2f;
            Assert.That(Target("ren").Interact(), Is.True);
            Facing(ren, 2f);
            Assert.That(Quaternion.Angle(body, ren.transform.rotation), Is.LessThan(0.1f));
            Assert.That(ren.transform.position, Is.EqualTo(position));
            Assert.That(Target("ren").transform.position, Is.EqualTo(targetPosition));
            var look = Quaternion.Inverse(body) * head.rotation * Quaternion.Inverse(reference);
            Assert.That(Signed(look.eulerAngles.y), Is.InRange(50f, 75f));
            Assert.That(Signed(look.eulerAngles.x), Is.InRange(-30f, -10f));
            // A frozen listening clip must not accumulate an extra turn on each update.
            var first = head.rotation;
            for (var i = 0; i < 20; i++) ren.TickConversationFacing(0.1f);
            Assert.That(Quaternion.Angle(first, head.rotation), Is.LessThan(0.1f));
        }

        [Test]
        public void EscapeSwitchAndLoadResetReleaseFacingAndRestoreStandingOrientation()
        {
            var ivo = Resident("ivo");
            var ren = Resident("ren");
            var ivoRotation = ivo.transform.rotation;
            var renRotation = ren.transform.rotation;
            view.transform.position = ivo.transform.position + Vector3.right * 2f + Vector3.up * 1.6f;
            Target("ivo").Interact(); Facing(ivo, 2f);
            Target("ren").Interact(); Facing(ivo, 2f); Facing(ren, 2f);
            Assert.That(ivo.IsFacingPlayer, Is.False);
            Assert.That(Quaternion.Angle(ivoRotation, ivo.transform.rotation), Is.LessThan(0.1f));
            Assert.That(ren.IsFacingPlayer, Is.True);
            dialogue.ResetAfterStateLoad(); Facing(ren, 2f);
            Assert.That(ren.IsFacingPlayer, Is.False);
            Assert.That(Quaternion.Angle(renRotation, ren.transform.rotation), Is.LessThan(0.1f));
            Target("ivo").Interact(); Facing(ivo, 2f);
            modal.HandleCancel(); Facing(ivo, 2f);
            Assert.That(ivo.IsFacingPlayer, Is.False);
            Assert.That(Quaternion.Angle(ivoRotation, ivo.transform.rotation), Is.LessThan(0.1f));
        }

        [Test]
        public void InterruptedWalkingResumesItsRouteFacingAndEscapePauseHoldsOrientation()
        {
            var ivo = Resident("ivo");
            Motion(ivo, 40f); Motion(ivo, 10f); Motion(ivo, 0.2f);
            var route = ivo.transform.rotation;
            var position = ivo.transform.position;
            view.transform.position = position + Vector3.forward * 2f + Vector3.up * 1.6f;
            Target("ivo").Interact(); Facing(ivo, 2f);
            Assert.That(Vector3.Dot(ivo.transform.forward, Vector3.forward), Is.GreaterThan(0.99f));
            Assert.That(ivo.transform.position, Is.EqualTo(position));
            dialogue.Close(); modal.HandleCancel();
            var held = ivo.transform.rotation;
            Facing(ivo, 2f);
            Assert.That(Quaternion.Angle(held, ivo.transform.rotation), Is.LessThan(0.1f));
            modal.HandleCancel(); Motion(ivo, 0.1f); Facing(ivo, 0.1f);
            Assert.That(Quaternion.Angle(route, ivo.transform.rotation), Is.LessThan(0.1f));
            Assert.That(ivo.transform.position, Is.Not.EqualTo(position));
        }

        [Test]
        public void SittingTransitionKeepsItsBodyOrientationDuringAConversation()
        {
            var ren = Resident("ren");
            Motion(ren, 40f); Motion(ren, 10f); Motion(ren, 3f); Motion(ren, 10f);
            Assert.That(ren.Phase, Is.EqualTo(ResidentMotionPhase.SittingDown));
            var rotation = ren.transform.rotation;
            view.transform.position = ren.transform.position + Vector3.right * 2f + Vector3.up * 1.6f;
            Target("ren").Interact(); Facing(ren, 2f);
            Assert.That(ren.IsFacingPlayer, Is.False);
            Assert.That(Quaternion.Angle(rotation, ren.transform.rotation), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator RenderStandingAndSeatedConversationFacing()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            foreach (var resident in residents)
            {
                var head = resident.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                view.transform.position = resident.transform.position + new Vector3(-2f, 1.4f, -0.7f);
                view.transform.LookAt(head.position);
                Target(resident.ResidentId).Interact();
                dialogue.SelectCurrent(); Facing(resident, 2f);
                yield return null; yield return null;
                Capture(resident.ResidentId + "-standing-facing");
                dialogue.Close(); Facing(resident, 2f);
                Seat(resident);
                view.transform.position = resident.transform.position + new Vector3(-2f, 1.3f, -0.7f);
                view.transform.LookAt(resident.transform.position + Vector3.up * 0.8f);
                Target(resident.ResidentId).Interact();
                dialogue.SelectCurrent(); Facing(resident, 2f);
                yield return null; yield return null;
                Capture(resident.ResidentId + "-seated-facing");
                dialogue.Close(); Facing(resident, 2f);
            }
        }

        private ResidentMotion Resident(string id) => residents.Single(item => item.ResidentId == id);
        private ResidentDialogueTarget Target(string id) => Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsSortMode.None).Single(item => item.ResidentId == id);
        private static float Signed(float angle) => Mathf.DeltaAngle(0f, angle);
        private static void Facing(ResidentMotion resident, float seconds)
        {
            resident.Tick(0f);
            resident.GetComponent<Animator>().Update(0.3f);
            resident.GetComponent<Animator>().Update(0.3f);
            resident.TickConversationFacing(seconds);
        }
        private static void Motion(ResidentMotion resident, float seconds)
        {
            resident.Tick(seconds);
            resident.GetComponent<Animator>().Update(Mathf.Max(seconds, 0.3f));
            resident.GetComponent<Animator>().Update(0.3f);
        }
        private static void Seat(ResidentMotion resident)
        {
            Motion(resident, 40f); Motion(resident, 10f); Motion(resident, 3f); Motion(resident, 10f); Motion(resident, 10f);
            Assert.That(resident.Phase, Is.EqualTo(ResidentMotionPhase.Seated));
        }
        private void Capture(string name)
        {
            var directory = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            Assert.That(directory, Is.Not.Null.And.Not.Empty);
            var render = new RenderTexture(800, 600, 24);
            var pixels = new Texture2D(800, 600, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                view.targetTexture = render;
                for (var i = 0; i < 3; i++) view.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, 800, 600), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), pixels.EncodeToPNG());
            }
            finally
            {
                view.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(render);
                Object.DestroyImmediate(pixels);
            }
        }
    }
}
