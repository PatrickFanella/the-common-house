using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public partial class ResidentAnimationTests
    {
        [UnityTest]
        public IEnumerator EachResidentPlaysItsOwnIdleOnAHumanoidRig()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            yield return null;
            var residents = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None)
                .Where(animator => animator.name.StartsWith("Resident "))
                .OrderBy(animator => animator.name)
                .ToList();
            Assert.That(residents, Has.Count.EqualTo(2), "Both resident positions need a character.");
            var idles = residents.Select(animator =>
            {
                Assert.That(animator.isHuman, Is.True, animator.name + " must use a humanoid avatar.");
                Assert.That(animator.runtimeAnimatorController, Is.Not.Null, animator.name + " needs a controller.");
                var clips = animator.GetCurrentAnimatorClipInfo(0);
                Assert.That(clips, Is.Not.Empty, animator.name + " is not playing a clip.");
                Assert.That(clips[0].clip.isLooping, Is.True, animator.name + " should loop its idle.");
                return clips[0].clip.name;
            }).ToList();
            Assert.That(idles, Is.EquivalentTo(new[] { "breathing-idle", "neutral-idle" }));
        }
    }
}
