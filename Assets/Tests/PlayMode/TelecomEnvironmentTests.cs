using System.Collections;
using System.Linq;
using CommonHouse.Opening;
using CommonHouse.Outpost;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class TelecomEnvironmentTests
    {
        private Transform dressing;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<OpeningHouseholdController>().Close();
            dressing = Object.FindAnyObjectByType<OutpostLayout>().transform.Find("OfficeConversion/TelecommunicationsAdaptation");
            Assert.That(dressing, Is.Not.Null);
        }

        [Test]
        public void CableRoutesReachEquipmentAndStayAboveWalkingHeight()
        {
            foreach (var id in new[] { "service trunk", "repair branch", "radio branch", "service drop", "line frame feed" })
                Assert.That(dressing.Find("Cable route: " + id), Is.Not.Null, id);
            foreach (var renderer in dressing.Find("Cable route: service trunk").GetComponentsInChildren<Renderer>())
                Assert.That(renderer.bounds.min.y, Is.GreaterThan(2.5f));
            foreach (var id in new[] { "repair", "radio", "trunk" }) Assert.That(dressing.Find("Junction: " + id), Is.Not.Null);
            Assert.That(dressing.Find("Junction: repair").position, Is.EqualTo(new Vector3(-11.4f, 1.05f, -14.1f)));
            Assert.That(dressing.Find("Junction: radio").position, Is.EqualTo(new Vector3(-11.4f, 1.15f, -2.8f)));
            Assert.That(dressing.Find("Patch panel: utility upper").GetComponentsInChildren<Transform>().Count(t => t.name == "Patch socket"), Is.EqualTo(16));
            Assert.That(dressing.Find("Former office cross-connect").GetComponentsInChildren<Transform>().Count(t => t.name == "Terminal block"), Is.EqualTo(6));
        }

        [Test]
        public void PrintsAreDistinctRetroAssetsWithoutCaseBehavior()
        {
            var textures = new System.Collections.Generic.HashSet<Texture>();
            foreach (var id in new[] { "patch-share", "keep-listening", "use-pass" })
            {
                var print = dressing.Find("Original print: " + id);
                Assert.That(print, Is.Not.Null);
                var material = print.Find("Printed paper").GetComponent<Renderer>().sharedMaterial;
                Assert.That(material.shader.name, Is.EqualTo("CommonHouse/Retro"));
                var texture = material.GetTexture("_MainTex");
                Assert.That(texture.width, Is.EqualTo(128));
                Assert.That(texture.height, Is.EqualTo(192));
                Assert.That(texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(textures.Add(texture), Is.True);
            }
            Assert.That(dressing.GetComponentsInChildren<MonoBehaviour>(), Is.Empty);
            Assert.That(dressing.GetComponentsInChildren<Collider>().Length, Is.EqualTo(1));
            var state = Object.FindAnyObjectByType<CaseSession>().State;
            foreach (var id in new[] { "E01", "E02", "E03", "E04", "E05", "E06" }) Assert.That(state.HasEvidence(id), Is.False);
        }

        [UnityTest]
        public IEnumerator RenderInfrastructureAndOriginalPrints()
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null), "Use --graphics for this fixture.");
            var camera = Object.FindAnyObjectByType<FirstPersonMovement>().GetComponentInChildren<Camera>();
            foreach (var view in new[]
            {
                ("telecom-utility", new Vector3(9.4f, 1.6f, -12.5f), new Vector3(7.2f, 1.6f, -14.8f)),
                ("telecom-line-frame", new Vector3(10.3f, 1.6f, -13.3f), new Vector3(13.68f, 1.5f, -14.5f)),
                ("telecom-concourse", new Vector3(-3f, 1.6f, -5f), new Vector3(-7.5f, 2f, -7.7f)),
                ("telecom-repair-print", new Vector3(-11f, 1.6f, -10f), new Vector3(-13.78f, 1.7f, -10f)),
                ("telecom-commons-print", new Vector3(2f, 1.6f, -9.5f), new Vector3(4.78f, 1.6f, -10f)),
            })
            {
                camera.transform.position = view.Item2;
                camera.transform.LookAt(view.Item3);
                yield return null; yield return null;
                OfficeComplexTests.Capture(camera, view.Item1);
            }
        }
    }
}
