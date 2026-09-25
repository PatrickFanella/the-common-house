namespace CommonHouse.Interaction
{
    /// <summary>Non-case-specific copy for the existing keyboard controls.</summary>
    public static class ControlsReference
    {
        public const string SharedText =
            "[WASD] Move  [Arrow keys] Look\n" +
            "[E] Use the focused object  [Enter] Read, ask, or confirm\n" +
            "[R] Review received transcripts  [N] Notebook  [H] Hint\n" +
            "[F5] Save  [F8] Checkpoint  [F9] Load\n" +
            "[PgUp/PgDn] Scroll  [-/=] Text size";

        public const string WelcomeText = "Controls\n" + SharedText + "\n[Escape] Pause or close the current panel.";
        public const string PauseText = "Paused\n" + SharedText + "\n[Escape] Resume.";
    }
}
