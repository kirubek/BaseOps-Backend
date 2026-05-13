namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Reasons for task deferral
    /// </summary>
    public enum CarryOverDeferralReason
    {
        WaitingForPart = 1,
        DueToTool = 2,
        GroundTimeReduction = 3,
        ManpowerShortage = 4,
        PlanningPurpose = 5,
        EngineeringReview = 6,
        OperationalRequirement = 7,
        VendorDelay = 8,
        MaterialUnavailable = 9,
        AwaitingApproval = 10,
        Other = 11
    }
}
