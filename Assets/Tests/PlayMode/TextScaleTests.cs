using CommonHouse.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace CommonHouse.Tests
{
    public class TextScaleTests
    {
        [Test]
        public void MaximumScaleWrapsLongCaptionedCaseText()
        {
            CaseUi.TextScale = CaseUi.MaximumTextScale;
            var panel = CaseUi.PanelForResolution(800f, 600f);
            var style = CaseUi.CreateWrappedStyle(new GUIStyle());
            Assert.That(style.fontSize, Is.EqualTo(24));
            Assert.That(style.CalcHeight(new GUIContent("Ada's transcript. [Kitchen bell sounds on the recording.] Ren's private response and send confirmation remain readable."), panel.width - 56f), Is.GreaterThan(style.lineHeight));
            Assert.That(panel.yMax, Is.LessThanOrEqualTo(588f));
            CaseUi.TextScale = 1f;
        }

        [Test]
        public void TextScaleClampsAndPanelKeepsMargins()
        {
            CaseUi.TextScale = 99f;
            Assert.That(CaseUi.TextScale, Is.EqualTo(CaseUi.MaximumTextScale));
            CaseUi.TextScale = .1f;
            Assert.That(CaseUi.TextScale, Is.EqualTo(CaseUi.MinimumTextScale));
            var panel = CaseUi.PanelForResolution(800f, 600f);
            Assert.That(panel.x, Is.GreaterThanOrEqualTo(12f));
            Assert.That(panel.y, Is.GreaterThanOrEqualTo(12f));
        }

        [Test]
        public void KeyboardScrollKeysAreDeterministicAndDoNotChangeTextScale()
        {
            CaseUi.TextScale = 1.5f;
            var down = CaseUi.ApplyScrollKey(Vector2.zero, KeyCode.PageDown);
            Assert.That(down.y, Is.EqualTo(180f));
            Assert.That(CaseUi.ApplyScrollKey(down, KeyCode.PageUp).y, Is.EqualTo(0f));
            Assert.That(CaseUi.ApplyScrollKey(down, KeyCode.Home).y, Is.EqualTo(0f));
            Assert.That(CaseUi.ApplyScrollKey(down, KeyCode.End).y, Is.EqualTo(1000000f));
            CaseUi.TextScale = 1f;
        }

    }
}
