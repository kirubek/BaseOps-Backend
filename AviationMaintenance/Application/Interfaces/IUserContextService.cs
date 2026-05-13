using System;

namespace AviationMaintenance.Application.Interfaces
{
    /// <summary>
    /// Service for accessing current user context information
    /// </summary>
    public interface IUserContextService
    {
        Guid EmployeeId { get; }
        string Role { get; }
        Guid? SectionId { get; }
        Guid? HangarId { get; }
        Guid? ShopId { get; }
    }
}
