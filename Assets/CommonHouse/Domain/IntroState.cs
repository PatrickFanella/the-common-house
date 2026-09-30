using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;

namespace CommonHouse.Domain
{
    public sealed class IntroTranscriptLine
    {
        public string Id { get; }
        public string SpeakerName { get; }
        public string Text { get; }
        internal IntroTranscriptLine(IntroLine line, string speakerName)
        {
            Id = line.Id;
            SpeakerName = speakerName;
            Text = line.Text;
        }
    }

    // A readable review, not an audio player or a running cinematic.
    public sealed class IntroReview
    {
        internal object Owner { get; }
        internal long Sequence { get; }
        internal string PreferredAddress { get; }
        internal bool AddressSelected { get; }
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList<IntroTranscriptLine> Lines { get; }
        public IReadOnlyList<IntroSource> Sources { get; }
        public IReadOnlyList<IntroChoice> Choices { get; }
        public IReadOnlyDictionary<string, string> SelectedOptions { get; }

        internal IntroReview(object owner, long sequence, string id, string title, string description,
            IEnumerable<IntroTranscriptLine> lines, IEnumerable<IntroSource> sources, IEnumerable<IntroChoice> choices,
            IDictionary<string, string> selectedOptions, bool addressSelected = false, string preferredAddress = null)
        {
            Owner = owner;
            Sequence = sequence;
            Id = id;
            Title = title;
            Description = description;
            Lines = Array.AsReadOnly(lines.ToArray());
            Sources = Array.AsReadOnly(sources.ToArray());
            Choices = Array.AsReadOnly(choices.ToArray());
            SelectedOptions = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(selectedOptions, StringComparer.Ordinal));
            AddressSelected = addressSelected;
            PreferredAddress = preferredAddress;
        }
    }

    // Isolated in-memory orientation. No CaseState, IO, clock, save, or transmission effects.
    public sealed class IntroState
    {
        private readonly IntroContent content;
        private readonly object owner = new object();
        private long sequence;
        private readonly HashSet<string> reviewedLines = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> acquiredSources = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> acknowledgedScenes = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> expressions = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, long> events = new Dictionary<string, long>(StringComparer.Ordinal);
        private static readonly string[] orientationSources = { "IN-C01", "IN-C02", "IN-C03", "IN-T05", "IN-C04" };
        private static readonly string[] shelterLines = { "IN-L081", "IN-L084", "IN-L085" };

        public IntroReview ActiveReview { get; private set; }
        public bool IsPaused { get; private set; }
        public bool TranscriptOnly { get; private set; }
        public bool WarningAcknowledged => acquiredSources.Contains("IN-C01");
        public bool NetworkAcknowledged => acquiredSources.Contains("IN-C03");
        public bool ShelterAcknowledged => shelterLines.All(reviewedLines.Contains);
        public bool GreetingReviewed => acquiredSources.Contains("IN-T05");
        public bool IsComplete { get; private set; }
        public string PreferredAddress { get; private set; }
        public IReadOnlyList<string> ReviewedLineIds => Array.AsReadOnly(reviewedLines.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        public IReadOnlyList<string> AcquiredSourceIds => Array.AsReadOnly(acquiredSources.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        public IReadOnlyList<string> AcknowledgedSceneIds => Array.AsReadOnly(acknowledgedScenes.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        public IReadOnlyDictionary<string, string> Expressions => new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(expressions, StringComparer.Ordinal));
        public IReadOnlyList<string> MissingCoreAcknowledgments => Array.AsReadOnly(new[] {
            WarningAcknowledged ? null : "warning", NetworkAcknowledged ? null : "network",
            ShelterAcknowledged ? null : "shelter", GreetingReviewed ? null : "greeting"
        }.Where(value => value != null).ToArray());

        public IntroState(IntroContent content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        private IntroScene Scene(string id) => content.Scenes.SingleOrDefault(item => item.Id == id)
            ?? throw new ArgumentException("Unknown intro scene.", nameof(id));
        private IntroSource Source(string id) => content.Sources.SingleOrDefault(item => item.Id == id)
            ?? throw new ArgumentException("Unknown intro source.", nameof(id));
        private IntroTranscriptLine Label(string id)
        {
            var line = content.Lines.Single(item => item.Id == id);
            return new IntroTranscriptLine(line, content.Speakers.Single(item => item.Id == line.SpeakerId).Name);
        }

        private IntroReview Review(string id, string title, string description, IEnumerable<string> lineIds,
            IEnumerable<IntroSource> sources, IEnumerable<IntroChoice> choices, IDictionary<string, string> selections,
            bool addressSelected = false, string preferredAddress = null)
        {
            return new IntroReview(owner, ++sequence, id, title, description, lineIds.Distinct().Select(Label),
                sources, choices, selections, addressSelected, preferredAddress);
        }

        private IntroReview Open(IntroReview review)
        {
            if (ActiveReview != null) throw new InvalidOperationException("Close or acknowledge the current review first.");
            ActiveReview = review;
            IsPaused = false;
            TranscriptOnly = false;
            return review;
        }

        private void Current(IntroReview review)
        {
            if (review == null || !ReferenceEquals(review.Owner, owner) || !ReferenceEquals(review, ActiveReview))
                throw new InvalidOperationException("Review is not the current presented content.");
        }

        public IntroReview OpenScene(string sceneId)
        {
            var scene = Scene(sceneId);
            var choices = scene.ChoiceIds.Select(id => content.Choices.Single(item => item.Id == id)).ToArray();
            var conditionalLines = new HashSet<string>(choices.SelectMany(choice => choice.Options).SelectMany(option => option.ResponseLineIds));
            var lines = scene.LineIds.Where(id => !conditionalLines.Contains(id) && content.Lines.Single(line => line.Id == id).SpeakerId != "player");
            return Open(Review(scene.Id, scene.Title, scene.Description, lines, scene.SourceIds.Select(Source), choices,
                new Dictionary<string, string>()));
        }

        public IntroReview OpenSource(string sourceId)
        {
            var source = Source(sourceId);
            return Open(Review(source.Id, source.Id, source.Description, source.LineIds, new[] { source },
                Array.Empty<IntroChoice>(), new Dictionary<string, string>()));
        }

        public IntroReview OpenOrientation()
        {
            var sources = orientationSources.Select(Source).ToArray();
            return Open(Review("orientation", "Orientation", "Review the shelter offer and attributed accounts. The office greeting is stored playback; its sender and date remain unknown.",
                shelterLines.Concat(sources.SelectMany(source => source.LineIds)), sources, Array.Empty<IntroChoice>(),
                new Dictionary<string, string>()));
        }

        public IntroReview SelectOption(IntroReview review, string choiceId, string optionId, string preferredAddress = null)
        {
            Current(review);
            if (IsPaused) throw new InvalidOperationException("Resume before choosing a response.");
            var choice = review.Choices.SingleOrDefault(item => item.Id == choiceId)
                ?? throw new ArgumentException("Choice is not part of this scene.", nameof(choiceId));
            var option = choice.Options.SingleOrDefault(item => item.Id == optionId)
                ?? throw new ArgumentException("Unknown choice option.", nameof(optionId));
            if (preferredAddress != null && optionId != "IN-Q06-A")
                throw new ArgumentException("Preferred address requires the explicit name option.", nameof(preferredAddress));
            if (preferredAddress != null && (string.IsNullOrWhiteSpace(preferredAddress) || preferredAddress.Any(char.IsControl)))
                throw new ArgumentException("Preferred address must be nonempty single-line text.", nameof(preferredAddress));
            var selections = review.SelectedOptions.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
            selections[choiceId] = optionId;
            // Preserve every response already presented in this review, including earlier optional questions.
            ActiveReview = Review(review.Id, review.Title, review.Description, review.Lines.Select(line => line.Id).Concat(option.ResponseLineIds),
                review.Sources, review.Choices, selections, review.AddressSelected || choiceId == "IN-Q06",
                choiceId == "IN-Q06" ? preferredAddress?.Trim() : review.PreferredAddress);
            return ActiveReview;
        }

        public void Pause(IntroReview review) { Current(review); IsPaused = true; }
        public void Resume(IntroReview review) { Current(review); IsPaused = false; }
        public void SkipToTranscript(IntroReview review) { Current(review); TranscriptOnly = true; }
        public void Close(IntroReview review)
        {
            Current(review);
            ActiveReview = null;
            IsPaused = false;
            TranscriptOnly = false;
        }

        public bool Acknowledge(string eventId, IntroReview review)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("Acknowledgment requires an event ID.", nameof(eventId));
            if (review == null || !ReferenceEquals(review.Owner, owner)) throw new InvalidOperationException("Review belongs to another orientation.");
            if (events.TryGetValue(eventId, out var previous))
            {
                if (previous != review.Sequence) throw new InvalidOperationException("Event ID reused for different presented content.");
                return false;
            }
            Current(review);
            if (IsPaused) throw new InvalidOperationException("Resume before acknowledging content.");
            var finishing = review.Id == "IN-S07";
            if (finishing && (!review.SelectedOptions.ContainsKey("IN-Q03") || MissingCoreAcknowledgments.Count > 0))
                throw new InvalidOperationException("Review the core orientation and choose a final response before completing.");

            var changed = false;
            foreach (var line in review.Lines) changed |= reviewedLines.Add(line.Id);
            // Source acquisition requires its whole transcript; partial answers never become a complete account.
            foreach (var source in review.Sources)
                if (source.Kind == "document" || source.LineIds.All(reviewedLines.Contains)) changed |= acquiredSources.Add(source.Id);
            if (content.Scenes.Any(scene => scene.Id == review.Id)) changed |= acknowledgedScenes.Add(review.Id);
            foreach (var selection in review.SelectedOptions)
            {
                changed |= !expressions.TryGetValue(selection.Key, out var previousOption) || previousOption != selection.Value;
                expressions[selection.Key] = selection.Value;
            }
            if (review.AddressSelected)
            {
                changed |= PreferredAddress != review.PreferredAddress;
                PreferredAddress = review.PreferredAddress;
            }
            if (finishing && !IsComplete) { IsComplete = true; changed = true; }
            events.Add(eventId, review.Sequence);
            Close(review);
            return changed;
        }

        public IntroSource ReplaySource(string sourceId)
        {
            var source = Source(sourceId);
            if (!acquiredSources.Contains(sourceId)) throw new InvalidOperationException("Source has not been acknowledged.");
            return source;
        }
    }
}
