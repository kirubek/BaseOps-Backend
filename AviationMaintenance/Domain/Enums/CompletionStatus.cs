namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Status of task completion
    /// </summary>
    public enum CompletionStatus
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        Verified = 4,
        Closed = 5
    }
}
