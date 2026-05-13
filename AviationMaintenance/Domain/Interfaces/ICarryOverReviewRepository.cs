using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for CarryOverReview operations
    /// </summary>
    public interface ICarryOverReviewRepository
    {
        Task<CarryOverReview?> GetByIdAsync(Guid id);
        Task<IEnumerable<CarryOverReview>> GetByReportIdAsync(Guid reportId);
        Task<IEnumerable<CarryOverReview>> GetByReviewerIdAsync(Guid reviewerId);
        Task<CarryOverReview> CreateAsync(CarryOverReview review);
        Task<CarryOverReview> UpdateAsync(CarryOverReview review);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
