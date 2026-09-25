using System;
using CommonHouse.Content;
using CommonHouse.Domain;
using UnityEngine;

namespace CommonHouse.Runtime
{
    /// <summary>One scene-local case session. Persistence may replace State after a validated load.</summary>
    public sealed class CaseSession : MonoBehaviour
    {
        public CaseContent Content { get; private set; }
        public CaseState State { get; private set; }

        private void Awake()
        {
            if (Content == null) Content = CaseContent.LoadBaseline();
            if (State == null) State = new CaseState(Content);
        }

        public void Configure(CaseContent content, CaseState state)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void ReplaceState(CaseState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}
