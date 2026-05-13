using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AviationMaintenance.Application.DTOs.Bulletin;

namespace AviationMaintenance.Application.Interfaces
{
    /// <summary>
    /// Service interface for bulletin operations
    /// </summary>
    public interface IBulletinService
    {
        Task<BulletinResponse> CreateBulletinAsync(CreateBulletinRequest request);
        Task<BulletinResponse> UpdateBulletinAsync(UpdateBulletinRequest request);
        Task<BulletinResponse?> GetBulletinByIdAsync(Guid id);
        Task<BulletinListResponse> ListBulletinsAsync(BulletinListRequest request);
        Task<bool> DeleteBulletinAsync(Guid id);
        Task<bool> ActivateBulletinAsync(Guid id);
        Task<bool> DeactivateBulletinAsync(Guid id);
        Task<bool> MarkBulletinAsReadAsync(Guid bulletinId);
        Task<int> GetUnreadCountAsync();
        Task<int> GetHighPriorityUnreadCountAsync();
        Task<List<BulletinResponse>> GetDashboardBulletinsAsync(int maxCount = 10);
        Task<BulletinAnalyticsResponse?> GetBulletinAnalyticsAsync(Guid bulletinId);
        Task<BulletinListResponse> GetExpiredBulletinsAsync(BulletinListRequest request);
        Task<List<BulletinAnalyticsResponse>> GetHighPriorityBulletinsAnalyticsAsync();
    }
}
