using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class ProjectPlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator BootstrapLoadsAndUsesDisposablePersistence()
        {
            Assert.That(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), Is.EqualTo("test"));
            Assert.That(Application.persistentDataPath.Replace('\\', '/'), Does.Contain("/CommonHouseVerification/run-"));
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Bootstrap"));
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Camera.main.isActiveAndEnabled, Is.True);
            Directory.CreateDirectory(Application.persistentDataPath);
            var sentinel = Path.Combine(Application.persistentDataPath, "runner-smoke.txt");
            try
            {
                File.WriteAllText(sentinel, "disposable");
                yield return null;
                Assert.That(File.ReadAllText(sentinel), Is.EqualTo("disposable"));
            }
            finally
            {
                if (File.Exists(sentinel)) File.Delete(sentinel);
            }
        }
    }
}
