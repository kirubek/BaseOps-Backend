using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for CarryOverTask operations
    /// </summary>
    public interface ICarryOverTaskRepository
    {
        Task<CarryOverTask?> GetByIdAsync(Guid id);
        Task<IEnumerable<CarryOverTask>> GetByReportIdAsync(Guid reportId);
        Task<IEnumerable<CarryOverTask>> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<CarryOverTask>> GetByDeferralReasonAsync(CarryOverDeferralReason reason);
        Task<CarryOverTask> CreateAsync(CarryOverTask task);
        Task<CarryOverTask> UpdateAsync(CarryOverTask task);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> BarcodeExistsInReportAsync(Guid reportId, string barcode);
        Task<int> GetNextItemNumberAsync(Guid reportId);
        Task BulkCreateAsync(IEnumerable<CarryOverTask> tasks);
        Task BulkUpdateAsync(IEnumerable<CarryOverTask> tasks);
    }
}
