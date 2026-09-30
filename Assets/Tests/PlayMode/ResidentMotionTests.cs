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
    public partial class ResidentAnimationTests
    {
        private ResidentMotion[] residents;
        private DialoguePresenter dialogue;
        private ModalInputCoordinator modal;

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
            Object.FindAnyObjectByType<CaseSession>().State.AcquireSource("animation-test-call", "T01");
        }

        [Test]
        public void WalkSitStandAndReturnUseRealClipsAndKeepTargetsWithTheBody()
        {
            foreach (var resident in residents)
            {
                Tick(resident, 40f);
                AssertPhase(resident, ResidentMotionPhase.StartingWalk, "start-walking");
                Tick(resident, 10f);
                AssertPhase(resident, ResidentMotionPhase.Walking, "walking");
                Tick(resident, 0.4f);
                Assert.That(Vector3.Distance(resident.transform.position, resident.StandingPosition), Is.GreaterThan(0.1f));
                AssertBodyAligned(resident);
                Tick(resident, 3f);
                AssertPhase(resident, ResidentMotionPhase.StoppingWalk, "stop-walking");
                Tick(resident, 10f);
                AssertPhase(resident, ResidentMotionPhase.SittingDown, "stand-to-sit");
                Tick(resident, 10f);
                AssertPhase(resident, ResidentMotionPhase.Seated, "sitting-idle");
                Assert.That(resident.transform.position, Is.EqualTo(resident.SeatPosition));
                AssertBodyAligned(resident);
                Assert.That(resident.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position.y, Is.LessThan(0.9f), "The seated clip must lower the body onto the chair.");
                var target = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsSortMode.None)
                    .Single(item => item.ResidentId == resident.ResidentId);
                Assert.That(target.transform.position, Is.EqualTo(resident.transform.position + Vector3.up * 0.85f));
                if (resident.ResidentId == "ren") Assert.That(resident.transform.position.x, Is.InRange(10.5f, 13f));
                Tick(resident, 30f);
                AssertPhase(resident, ResidentMotionPhase.StandingUp, "sit-to-stand");
                Tick(resident, 10f);
                AssertPhase(resident, ResidentMotionPhase.StartingWalk, "start-walking");
                Tick(resident, 10f);
                Tick(resident, 3f);
                Tick(resident, 10f);
                AssertPhase(resident, ResidentMotionPhase.Standing, null);
                Assert.That(resident.transform.position, Is.EqualTo(resident.StandingPosition));
                Assert.That(resident.GetComponent<Animator>().applyRootMotion, Is.False);
            }
        }

        [Test]
        public void OnlyTheActiveRespondingResidentTalksAndEscapeClearsIt()
        {
            var ivo = residents.Single(item => item.ResidentId == "ivo");
            var ren = residents.Single(item => item.ResidentId == "ren");
            Assert.That(Target("ivo").Interact(), Is.True);
            TickAll(1f);
            Assert.That(residents.All(item => !item.IsTalking), Is.True, "Question selection is listening.");
            Assert.That(dialogue.SelectCurrent(), Is.True);
            TickAll(1f);
            Assert.That(ivo.IsTalking, Is.True);
            Assert.That(ren.IsTalking, Is.False);
            AssertClip(ivo, "talking");
            Assert.That(Target("ren").Interact(), Is.True);
            Assert.That(dialogue.SelectCurrent(), Is.True);
            TickAll(1f);
            Assert.That(ivo.IsTalking, Is.False);
            Assert.That(ren.IsTalking, Is.True);
            AssertClip(ren, "talking");
            modal.HandleCancel();
            TickAll(0f);
            Assert.That(residents.All(item => !item.IsTalking), Is.True);
            Assert.That(dialogue.ActiveResidentId, Is.Null);
            Assert.That(ren.GetComponent<Animator>().speed, Is.EqualTo(1f));
        }

        [Test]
        public void SeatedConversationUsesSeatedTalkingAndLoadResetClearsIt()
        {
            var ren = residents.Single(item => item.ResidentId == "ren");
            Seat(ren);
            Assert.That(Target("ren").Interact(), Is.True);
            Assert.That(dialogue.SelectCurrent(), Is.True);
            TickAll(1f);
            AssertClip(ren, "sitting-talking");
            Assert.That(ren.Phase, Is.EqualTo(ResidentMotionPhase.Seated));
            Assert.That(ren.transform.position, Is.EqualTo(ren.SeatPosition));
            dialogue.ResetAfterStateLoad();
            TickAll(0f);
            Assert.That(ren.IsTalking, Is.False);
            AssertClip(ren, "sitting-idle");
        }

        [Test]
        public void PauseAndOtherPanelsFreezeTravelAndResumeWithoutTeleporting()
        {
            var ivo = residents.Single(item => item.ResidentId == "ivo");
            Tick(ivo, 40f); Tick(ivo, 10f); Tick(ivo, 0.3f);
            var position = ivo.transform.position;
            modal.HandleCancel();
            Tick(ivo, 100f);
            Assert.That(ivo.transform.position, Is.EqualTo(position));
            Assert.That(ivo.GetComponent<Animator>().speed, Is.Zero);
            modal.HandleCancel();
            var panel = new OtherPanel();
            Assert.That(modal.TryOpen(panel), Is.True);
            Tick(ivo, 100f);
            Assert.That(ivo.transform.position, Is.EqualTo(position));
            modal.Release(panel);
            Tick(ivo, 0.1f);
            Assert.That(Vector3.Distance(ivo.transform.position, position), Is.EqualTo(0.065f).Within(0.001f));
        }

        [Test]
        public void WalkingResidentYieldsToPlayerAndConversationHoldsItsPosition()
        {
            var ivo = residents.Single(item => item.ResidentId == "ivo");
            Tick(ivo, 40f); Tick(ivo, 10f);
            var position = ivo.transform.position;
            var playerCapsule = modal.GetComponent<CharacterController>();
            playerCapsule.enabled = false;
            modal.transform.position = position + Vector3.left * 0.4f;
            Tick(ivo, 0.3f);
            Assert.That(ivo.transform.position, Is.EqualTo(position));
            Assert.That(Target("ivo").Interact(), Is.True);
            Assert.That(dialogue.SelectCurrent(), Is.True);
            Tick(ivo, 100f);
            Assert.That(ivo.transform.position, Is.EqualTo(position));
            AssertClip(ivo, "talking");
            dialogue.Close();
            modal.transform.position = new Vector3(-10f, 0f, 0f);
            Tick(ivo, 0.1f);
            Assert.That(ivo.Phase, Is.EqualTo(ResidentMotionPhase.Walking));
            Assert.That(ivo.transform.position, Is.Not.EqualTo(position));
            AssertClip(ivo, "walking");
        }

        [Test]
        public void SeatedClipsKeepHipsAtChairHeightThroughoutTheirLoops()
        {
            var resident = residents.Single(item => item.ResidentId == "ren");
            var animator = resident.GetComponent<Animator>();
            foreach (var clip in new[] { "sitting-idle", "sitting-talking" })
            {
                foreach (var time in new[] { 0.25f, 0.5f, 0.75f })
                {
                    animator.speed = 1f;
                    animator.Play(clip, 0, time);
                    animator.Update(0f);
                    animator.Update(0f);
                    var height = animator.GetBoneTransform(HumanBodyBones.Hips).position.y;
                    Assert.That(height, Is.InRange(0.35f, 0.65f), clip + " should stay at chair height.");
                }
            }
        }

        [UnityTest]
        public IEnumerator RenderedPosesWaitForSkinningToUpdate()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            foreach (var resident in residents)
            {
                Assert.That(Target(resident.ResidentId).Interact(), Is.True);
                Assert.That(dialogue.SelectCurrent(), Is.True);
                TickAll(0f);
                View(resident);
                yield return null;
                yield return null;
                Capture(resident, "talking");
                dialogue.Close(); TickAll(0f);
                Tick(resident, 40f); Tick(resident, 10f); Tick(resident, 0.4f);
                View(resident);
                yield return null;
                yield return null;
                Capture(resident, "walking");
                Tick(resident, 3f); Tick(resident, 10f); Tick(resident, 10f);
                View(resident);
                yield return null;
                yield return null;
                Capture(resident, "seated");
                Assert.That(Target(resident.ResidentId).Interact(), Is.True);
                Assert.That(dialogue.SelectCurrent(), Is.True);
                TickAll(0f);
                View(resident);
                yield return null;
                yield return null;
                Capture(resident, "sitting-talking");
                dialogue.Close(); TickAll(0f);
            }
        }

        private static void View(ResidentMotion resident)
        {
            var camera = Object.FindAnyObjectByType<Camera>();
            var target = resident.transform.position + Vector3.up * 0.8f;
            camera.transform.position = target + new Vector3(-2f, 0.6f, -0.3f);
            camera.transform.LookAt(target);
        }

        private ResidentDialogueTarget Target(string id) => Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsSortMode.None).Single(item => item.ResidentId == id);
        private void TickAll(float seconds) { foreach (var resident in residents) Tick(resident, seconds); }
        private static void Tick(ResidentMotion resident, float seconds)
        {
            resident.Tick(seconds);
            resident.GetComponent<Animator>().Update(Mathf.Max(seconds, 0.3f));
            resident.GetComponent<Animator>().Update(0.3f);
        }
        private static void Seat(ResidentMotion resident)
        {
            Tick(resident, 40f); Tick(resident, 10f); Tick(resident, 3f); Tick(resident, 10f); Tick(resident, 10f);
        }
        private static void AssertBodyAligned(ResidentMotion resident)
        {
            var delta = resident.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position - resident.transform.position;
            delta.y = 0f;
            Assert.That(delta.magnitude, Is.LessThan(0.25f), "The visible body must stay with its object and interaction target.");
        }

        private static void AssertPhase(ResidentMotion resident, ResidentMotionPhase phase, string clip)
        {
            Assert.That(resident.Phase, Is.EqualTo(phase));
            if (clip != null) AssertClip(resident, clip);
        }
        private static void AssertClip(ResidentMotion resident, string name)
        {
            Assert.That(resident.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Select(item => item.clip.name), Does.Contain(name));
        }
        private static void Capture(ResidentMotion resident, string pose)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var directory = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            Assert.That(directory, Is.Not.Null.And.Not.Empty);
            var camera = Object.FindAnyObjectByType<Camera>();
            var render = new RenderTexture(800, 600, 24);
            var pixels = new Texture2D(800, 600, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = render;
                for (var frame = 0; frame < 3; frame++) camera.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, 800, 600), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, resident.ResidentId + "-" + pose + ".png"), pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(render);
                Object.DestroyImmediate(pixels);
            }
        }
        private sealed class OtherPanel : IModalPanel { public void CloseFromModalInput() { } }
    }
}
