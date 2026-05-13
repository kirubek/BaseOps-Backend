using System;
using System.Threading.Tasks;
using AviationMaintenance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AviationMaintenance.Application.Interfaces
{
    /// <summary>
    /// Application database context interface for data access
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Section> Sections { get; }
        DbSet<Hangar> Hangars { get; }
        DbSet<Shop> Shops { get; }
        DbSet<Bulletin> Bulletins { get; }
        DbSet<BulletinAttachment> BulletinAttachments { get; }
        DbSet<BulletinReadStatus> BulletinReadStatuses { get; }
        DbSet<CarryOverReport> CarryOverReports { get; }
        DbSet<CarryOverTask> CarryOverTasks { get; }
        DbSet<CarryOverReview> CarryOverReviews { get; }

        Task<int> SaveChangesAsync();
    }
}
