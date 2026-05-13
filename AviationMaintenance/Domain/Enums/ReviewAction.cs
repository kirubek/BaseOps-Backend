namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Review actions for carry-over reports
    /// </summary>
    public enum ReviewAction
    {
        Approved = 1,
        Rejected = 2,
        RequiresChanges = 3,
        FlaggedForManagement = 4,
        Commented = 5
    }
}
