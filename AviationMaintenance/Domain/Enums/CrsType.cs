namespace AviationMaintenance.Domain.Enums
{
    /// <summary>
    /// Types of CRS (Corrective Action Request System)
    /// </summary>
    public enum CrsType
    {
        Corrective = 1,
        Preventive = 2,
        Emergency = 3,
        Scheduled = 4,
        Unscheduled = 5
    }
}
