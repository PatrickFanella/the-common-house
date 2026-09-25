namespace CommonHouse.Runtime
{
    /// <summary>Explicit pre-send checkpoint seam owned by the scene save runtime.</summary>
    public interface ICaseSaveCheckpoint
    {
        bool SaveCheckpoint();
    }

    /// <summary>Clears presentation state that is derived from a replaced CaseState.</summary>
    public interface ICaseStateLoadReset
    {
        void ResetAfterStateLoad();
    }
}
