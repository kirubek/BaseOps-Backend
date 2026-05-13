namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Status of carry-over tasks
    /// </summary>
    public enum CarryOverStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        OnHold = 5
    }
}
