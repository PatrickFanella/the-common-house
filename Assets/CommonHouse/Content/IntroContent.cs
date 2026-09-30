using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CommonHouse.Content
{
    [Serializable]
    public sealed class IntroSpeaker
    {
        [SerializeField] private string id, name;
        public string Id => id;
        public string Name => name;
    }

    [Serializable]
    public sealed class IntroLine
    {
        [SerializeField] private string id, speakerId, text;
        public string Id => id;
        public string SpeakerId => speakerId;
        public string Text => text;
    }

    [Serializable]
    public sealed class IntroSource
    {
        [SerializeField] private string id, kind, delivery, description, recordedAt;
        [SerializeField] private string[] lineIds;
        public string Id => id;
        public string Kind => kind;
        public string Delivery => delivery;
        public string Description => description;
        public string RecordedAt => recordedAt;
        public IReadOnlyList<string> LineIds => Array.AsReadOnly(lineIds ?? Array.Empty<string>());
    }

    [Serializable]
    public sealed class IntroOption
    {
        [SerializeField] private string id, text, nextSceneId;
        [SerializeField] private string[] responseLineIds;
        public string Id => id;
        public string Text => text;
        public string NextSceneId => nextSceneId;
        public IReadOnlyList<string> ResponseLineIds => Array.AsReadOnly(responseLineIds ?? Array.Empty<string>());
    }

    [Serializable]
    public sealed class IntroChoice
    {
        [SerializeField] private string id;
        [SerializeField] private IntroOption[] options;
        public string Id => id;
        public IReadOnlyList<IntroOption> Options => Array.AsReadOnly(options ?? Array.Empty<IntroOption>());
    }

    [Serializable]
    public sealed class IntroScene
    {
        [SerializeField] private string id, title, description, nextSceneId;
        [SerializeField] private string[] modes, lineIds, sourceIds, choiceIds;
        public string Id => id;
        public string Title => title;
        public string Description => description;
        public string NextSceneId => nextSceneId;
        public IReadOnlyList<string> Modes => Array.AsReadOnly(modes ?? Array.Empty<string>());
        public IReadOnlyList<string> LineIds => Array.AsReadOnly(lineIds ?? Array.Empty<string>());
        public IReadOnlyList<string> SourceIds => Array.AsReadOnly(sourceIds ?? Array.Empty<string>());
        public IReadOnlyList<string> ChoiceIds => Array.AsReadOnly(choiceIds ?? Array.Empty<string>());
    }

    /// <summary>Authored arrival content only. No session, acquisition, permission, or save state.</summary>
    [Serializable]
    public sealed class IntroContent
    {
        [SerializeField] private int version;
        [SerializeField] private string id;
        [SerializeField] private IntroSpeaker[] speakers;
        [SerializeField] private IntroLine[] lines;
        [SerializeField] private IntroSource[] sources;
        [SerializeField] private IntroScene[] scenes;
        [SerializeField] private IntroChoice[] choices;
        public int Version => version;
        public string Id => id;
        public IReadOnlyList<IntroSpeaker> Speakers => Array.AsReadOnly(speakers);
        public IReadOnlyList<IntroLine> Lines => Array.AsReadOnly(lines);
        public IReadOnlyList<IntroSource> Sources => Array.AsReadOnly(sources);
        public IReadOnlyList<IntroScene> Scenes => Array.AsReadOnly(scenes);
        public IReadOnlyList<IntroChoice> Choices => Array.AsReadOnly(choices);

        public static IntroContent Load()
        {
            var asset = Resources.Load<TextAsset>("Intro");
            if (asset == null) throw new InvalidOperationException("Missing Intro text asset.");
            return FromJson(asset.text);
        }

        public static IntroContent FromJson(string json)
        {
            Require(!string.IsNullOrWhiteSpace(json), "Empty intro content.");
            var content = JsonUtility.FromJson<IntroContent>(json);
            Require(content != null, "Missing intro content.");
            content.Validate();
            return content;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new ArgumentException(message);
        }

        private static HashSet<string> Ids<T>(IReadOnlyList<T> records, Func<T, string> key, string pattern, string label) where T : class
        {
            Require(records != null && records.Count > 0, "Missing " + label + ".");
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in records)
            {
                Require(record != null, "Null " + label + ".");
                var value = key(record);
                Require(value != null && Regex.IsMatch(value, pattern) && result.Add(value), "Invalid or duplicate " + label + " ID.");
            }
            return result;
        }

        private static void References(IReadOnlyList<string> values, HashSet<string> targets, string label, bool required = false)
        {
            Require(!required || values.Count > 0, "Missing " + label + ".");
            Require(values.Distinct(StringComparer.Ordinal).Count() == values.Count &&
                values.All(value => value != null && targets.Contains(value)), "Duplicate or dangling " + label + ".");
        }

        private void Validate()
        {
            Require(version == 1 && id == "intro", "Unsupported intro identity or version.");
            var speakerIds = Ids(speakers, item => item.Id, @"\A[a-z][a-z0-9-]*\z", "speaker");
            Require(speakers.All(item => !string.IsNullOrWhiteSpace(item.Name)), "Missing speaker name.");
            var lineIds = Ids(lines, item => item.Id, @"\AIN-L\d{3}\z", "line");
            Require(lines.All(item => !string.IsNullOrWhiteSpace(item.Text) && item.SpeakerId != null && speakerIds.Contains(item.SpeakerId)), "Missing line text or unknown speaker.");
            var sourceIds = Ids(sources, item => item.Id, @"\AIN-[TCD]\d{2}\z", "source");
            var sceneIds = Ids(scenes, item => item.Id, @"\AIN-S\d{2}\z", "scene");
            Require(sceneIds.SetEquals(Enumerable.Range(0, 8).Select(number => "IN-S" + number.ToString("D2"))), "Expected IN-S00 through IN-S07.");
            var choiceIds = Ids(choices, item => item.Id, @"\AIN-Q\d{2}\z", "choice");
            foreach (var source in sources)
            {
                Require(new[] { "transmission", "conversation", "document" }.Contains(source.Kind), "Unknown source kind.");
                Require(new[] { "recorded", "live", "account", "document" }.Contains(source.Delivery), "Unknown source delivery.");
                Require(!string.IsNullOrWhiteSpace(source.Description), "Missing accessible source description: " + source.Id);
                Require((source.Kind == "conversation" && source.Delivery == "account") ||
                    (source.Kind == "document" && source.Delivery == "document") ||
                    (source.Kind == "transmission" && new[] { "live", "recorded" }.Contains(source.Delivery)), "Source kind and delivery disagree.");
                References(source.LineIds, lineIds, "source transcript", source.Kind != "document");
            }
            Require(sourceIds.SetEquals(new[] { "IN-C01", "IN-C02", "IN-C03", "IN-C04", "IN-D01", "IN-T01", "IN-T02", "IN-T03", "IN-T04", "IN-T05" }), "Incomplete intro source set.");
            var greeting = sources.Single(item => item.Id == "IN-T05");
            Require(greeting.Kind == "transmission" && greeting.Delivery == "recorded" &&
                greeting.LineIds.SequenceEqual(new[] { "IN-L100", "IN-L101" }), "Office greeting must remain a complete recorded fragment.");
            var optionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in choices)
            {
                Require(choice.Options.Count >= 2, "Choice requires distinct alternatives.");
                foreach (var option in choice.Options)
                {
                    Require(option != null && option.Id != null && Regex.IsMatch(option.Id, @"\A" + Regex.Escape(choice.Id) + @"-[A-Z]\z") && optionIds.Add(option.Id), "Invalid or duplicate option ID.");
                    Require(!string.IsNullOrWhiteSpace(option.Text), "Missing option text.");
                    Require(option.NextSceneId != null && sceneIds.Contains(option.NextSceneId), "Dangling option scene.");
                    References(option.ResponseLineIds, lineIds, "option response", true);
                }
                Require(choice.Options.Select(item => item.Text).Distinct(StringComparer.Ordinal).Count() == choice.Options.Count, "Duplicate option text.");
            }
            var usedLines = new HashSet<string>(StringComparer.Ordinal);
            var usedSources = new HashSet<string>(StringComparer.Ordinal);
            var usedChoices = new HashSet<string>(StringComparer.Ordinal);
            foreach (var scene in scenes)
            {
                Require(!string.IsNullOrWhiteSpace(scene.Title) && !string.IsNullOrWhiteSpace(scene.Description), "Missing scene title or description.");
                Require(scene.Modes.Count > 0 && scene.Modes.Distinct().Count() == scene.Modes.Count &&
                    scene.Modes.All(mode => new[] { "play", "interact", "directed", "media", "choice" }.Contains(mode)), "Invalid scene mode.");
                References(scene.LineIds, lineIds, "scene line", true);
                References(scene.SourceIds, sourceIds, "scene source");
                References(scene.ChoiceIds, choiceIds, "scene choice");
                Require(string.IsNullOrEmpty(scene.NextSceneId) || sceneIds.Contains(scene.NextSceneId), "Dangling next scene.");
                foreach (var sourceId in scene.SourceIds)
                    Require(sources.Single(item => item.Id == sourceId).LineIds.All(scene.LineIds.Contains), "Source transcript outside its scene.");
                foreach (var choiceId in scene.ChoiceIds)
                    Require(choices.Single(item => item.Id == choiceId).Options.All(option => option.ResponseLineIds.All(scene.LineIds.Contains)), "Option response outside its scene.");
                usedLines.UnionWith(scene.LineIds);
                usedSources.UnionWith(scene.SourceIds);
                foreach (var choiceId in scene.ChoiceIds)
                    Require(usedChoices.Add(choiceId), "Choice belongs to multiple scenes.");
            }
            Require(usedLines.SetEquals(lineIds) && usedSources.SetEquals(sourceIds) && usedChoices.SetEquals(choiceIds), "Unreferenced intro content.");
        }
    }
}
