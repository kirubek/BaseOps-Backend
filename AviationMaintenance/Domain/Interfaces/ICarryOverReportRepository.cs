using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for CarryOverReport operations
    /// </summary>
    public interface ICarryOverReportRepository
    {
        Task<CarryOverReport?> GetByIdAsync(Guid id);
        Task<IEnumerable<CarryOverReport>> GetAllAsync();
        Task<IEnumerable<CarryOverReport>> GetBySectionAsync(Guid sectionId);
        Task<IEnumerable<CarryOverReport>> GetByAircraftAsync(string aircraftRegistration);
        Task<IEnumerable<CarryOverReport>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<CarryOverReport>> GetByStatusAsync(CarryOverReportStatus status);
        Task<CarryOverReport> CreateAsync(CarryOverReport report);
        Task<CarryOverReport> UpdateAsync(CarryOverReport report);
        Task<bool> DeleteAsync(Guid id);
        Task<int> CountAsync();
        Task<bool> ExistsAsync(Guid id);
    }
}
