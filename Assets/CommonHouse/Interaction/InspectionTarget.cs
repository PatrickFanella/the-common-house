using System;
using UnityEngine;

namespace CommonHouse.Interaction
{
    /// <summary>Readable object data. Opening it never records evidence.</summary>
    public sealed class InspectionTarget : MonoBehaviour
    {
        [SerializeField] private string inspectionId;
        [SerializeField] private string title;
        [TextArea(3, 12), SerializeField] private string readableText;
        [SerializeField] private string acquisitionId;

        public string InspectionId => inspectionId;
        public string Title => title;
        public string ReadableText => readableText;
        public string AcquisitionId => acquisitionId;
        public bool HasAcquisition => !string.IsNullOrWhiteSpace(acquisitionId);

        public void Configure(string id, string displayTitle, string transcript, string evidenceId = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Inspection ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayTitle)) throw new ArgumentException("Title is required.", nameof(displayTitle));
            if (string.IsNullOrWhiteSpace(transcript)) throw new ArgumentException("Readable text is required.", nameof(transcript));
            inspectionId = id;
            title = displayTitle;
            readableText = transcript;
            acquisitionId = evidenceId ?? string.Empty;
        }
    }
}
