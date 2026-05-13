namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Status of post-mortem reports
    /// </summary>
    public enum PostMortemStatus
    {
        Draft = 1,
        InReview = 2,
        Approved = 3,
        Published = 4,
        Archived = 5
    }
}
