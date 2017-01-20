namespace AgentFire.Sql.Tools
{
    public enum EntryMode
    {
        /// <summary>
        /// Submits all pending changes to the DataContext and commits used transaction automatically when a Dispose() it called.
        /// </summary>
        Automatic,

        /// <summary>
        /// Doesn't submit nor commit any changes. Call to Dispose() invokes the same call on used transaction, having it to roll back.
        /// </summary>
        Manual
    }
}
