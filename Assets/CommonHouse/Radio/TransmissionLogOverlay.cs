using UnityEngine;

namespace CommonHouse.Radio
{
    [RequireComponent(typeof(TransmissionLog))]
    public sealed class TransmissionLogOverlay : MonoBehaviour
    {
        private TransmissionLog log;
        private Vector2 scrollPosition;

        private void Awake()
        {
            log = GetComponent<TransmissionLog>();
        }

        private void OnGUI()
        {
            if (log == null || !log.IsOpen) return;
            var panel = new Rect(Screen.width * 0.16f, Screen.height * 0.12f, Screen.width * 0.68f, Screen.height * 0.76f);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            if (!log.IsShowingTranscript)
            {
                GUILayout.Label("Transmission log", GUI.skin.label);
                GUILayout.Space(12f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                for (var index = 0; index < log.Records.Count; index++)
                    GUILayout.Label((index == log.SelectedIndex ? "> " : "  ") + log.Records[index].SourceId + " — " + log.Records[index].SpeakerLabel, wrapped);
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.Label("[Up/Down] Select    [Enter] Open transcript    [Escape] Close", GUI.skin.label);
            }
            else
            {
                var record = log.SelectedRecord;
                GUILayout.Label(record.SourceId + " — " + record.SpeakerLabel, GUI.skin.label);
                GUILayout.Space(12f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                GUILayout.Label(record.Transcript, wrapped);
                if (!string.IsNullOrEmpty(record.SoundCaption)) GUILayout.Label(record.SoundCaption, wrapped);
                if (!string.IsNullOrEmpty(record.AssertedContactIdentity))
                    GUILayout.Label("Requested contact: " + record.AssertedContactIdentity + " (Ada's caller assertion; Ren/Mara identity is " +
                        (record.IsAssertedContactIdentityConfirmed ? "confirmed" : "unconfirmed") + ")", wrapped);
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.Label("[Escape] Return to transmission list", GUI.skin.label);
            }
            if (!string.IsNullOrEmpty(log.LastActionMessage)) GUILayout.Label(log.LastActionMessage, GUI.skin.label);
            GUILayout.EndArea();
        }
    }
}
