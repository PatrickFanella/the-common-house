using System;
using System.Collections.Generic;

namespace CommonHouse.Content
{
    /// <summary>Provisional, non-case-evidence opening copy kept apart from Case 01 records.</summary>
    public static class OpeningContent
    {
        public sealed class Line
        {
            public Line(string speaker, string text)
            {
                Speaker = speaker ?? throw new ArgumentNullException(nameof(speaker));
                Text = text ?? throw new ArgumentNullException(nameof(text));
            }

            public string Speaker { get; }
            public string Text { get; }
        }

        private static readonly Line[] Care =
        {
            new Line("Ren (provisional)", "I left food by the desk. Eat if you want; you do not owe anyone a performance."),
            new Line("Ren (provisional)", "The radio log keeps a transcript. You can replay a missed message without rushing."),
            new Line("Ivo (provisional)", "The headset is private when it is checked. The room speaker carries to everyone here. We can read the endpoint labels together."),
        };

        public static IReadOnlyList<Line> CareLines => Array.AsReadOnly(Care);
        public const string CallAvailableText = "A call is waiting whenever you are ready. There is no countdown.";
    }
}
