using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Radio
{
    [RequireComponent(typeof(TransmissionLog))]
    public sealed class TransmissionLogOverlay : MonoBehaviour
    {
        private TransmissionLog log;
        private Vector2 scrollPosition;
        private string lastSelectedSource;

        private void Awake()
        {
            log = GetComponent<TransmissionLog>();
        }

        private void OnGUI()
        {
            if (log == null || !log.IsOpen) return;
            if (lastSelectedSource != log.SelectedRecord?.SourceId)
            {
                scrollPosition = Vector2.zero;
                lastSelectedSource = log.SelectedRecord?.SourceId;
            }
            CaseUi.HandleKeyboardScale();
            var panel = CaseUi.Panel();
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            var wrapped = CaseUi.WrappedLabel();
            if (!log.IsShowingTranscript)
            {
                GUILayout.Label("Transmission log", CaseUi.WrappedLabel());
                GUILayout.Space(12f);
                scrollPosition = CaseUi.Scroll(scrollPosition); scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                for (var index = 0; index < log.Records.Count; index++)
                    GUILayout.Label((index == log.SelectedIndex ? "> " : "  ") + log.Records[index].SourceId + " — " + log.Records[index].SpeakerLabel, wrapped);
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.Label("[Up/Down] Select    [Enter] Open transcript    [Escape] Close", CaseUi.WrappedLabel());
            }
            else
            {
                var record = log.SelectedRecord;
                GUILayout.Label(record.SourceId + " — " + record.SpeakerLabel, CaseUi.WrappedLabel());
                GUILayout.Space(12f);
                scrollPosition = CaseUi.Scroll(scrollPosition); scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                GUILayout.Label(record.Transcript, wrapped);
                if (!string.IsNullOrEmpty(record.SoundCaption)) GUILayout.Label(record.SoundCaption, wrapped);
                if (!string.IsNullOrEmpty(record.AssertedContactIdentity))
                    GUILayout.Label("Requested contact: " + record.AssertedContactIdentity + " (Ada's caller assertion; Ren/Mara identity is " +
                        (record.IsAssertedContactIdentityConfirmed ? "confirmed" : "unconfirmed") + ")", wrapped);
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.Label("[Escape] Return to transmission list", CaseUi.WrappedLabel());
            }
            if (!string.IsNullOrEmpty(log.LastActionMessage)) GUILayout.Label(log.LastActionMessage, CaseUi.WrappedLabel());
            GUILayout.EndArea();
        }
    }
}
