using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AviationMaintenance.Domain.Entities;
using AviationMaintenance.Domain.Enums;
using AviationMaintenance.Domain.Interfaces;
using AviationMaintenance.Infrastructure.Data;

namespace AviationMaintenance.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for CarryOverTask operations
    /// </summary>
    public class CarryOverTaskRepository : ICarryOverTaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CarryOverTaskRepository> _logger;

        public CarryOverTaskRepository(ApplicationDbContext context, ILogger<CarryOverTaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarryOverTask?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Getting carry-over task by ID: {Id}", id);
            
            return await _context.CarryOverTasks
                .Include(t => t.Report)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<CarryOverTask>> GetByReportIdAsync(Guid reportId)
        {
            _logger.LogDebug("Getting carry-over tasks by report ID: {ReportId}", reportId);
            
            return await _context.CarryOverTasks
                .Include(t => t.Report)
                .Where(t => t.CarryOverReportId == reportId)
                .OrderBy(t => t.ItemNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverTask>> GetByBarcodeAsync(string barcode)
        {
            _logger.LogDebug("Getting carry-over tasks by barcode: {Barcode}", barcode);
            
            return await _context.CarryOverTasks
                .Include(t => t.Report)
                .Where(t => t.TaskBarcode == barcode)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverTask>> GetByDeferralReasonAsync(CarryOverDeferralReason reason)
        {
            _logger.LogDebug("Getting carry-over tasks by deferral reason: {Reason}", reason);
            
            return await _context.CarryOverTasks
                .Include(t => t.Report)
                .Where(t => t.DeferralReason == reason)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<CarryOverTask> CreateAsync(CarryOverTask task)
        {
            _logger.LogDebug("Creating carry-over task: {TaskBarcode}", task.TaskBarcode);
            
            await _context.CarryOverTasks.AddAsync(task);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created carry-over task with ID: {Id}", task.Id);
            
            return task;
        }

        public async Task<CarryOverTask> UpdateAsync(CarryOverTask task)
        {
            _logger.LogDebug("Updating carry-over task: {Id}", task.Id);
            
            _context.CarryOverTasks.Update(task);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated carry-over task with ID: {Id}", task.Id);
            
            return task;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting carry-over task: {Id}", id);
            
            var task = await _context.CarryOverTasks.FindAsync(id);
            if (task == null)
                return false;

            // Soft delete
            task.IsDeleted = true;
            task.ModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted carry-over task with ID: {Id}", id);
            
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogDebug("Checking if carry-over task exists: {Id}", id);
            
            return await _context.CarryOverTasks.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> BarcodeExistsInReportAsync(Guid reportId, string barcode)
        {
            _logger.LogDebug("Checking if barcode {Barcode} exists in report {ReportId}", barcode, reportId);
            
            return await _context.CarryOverTasks
                .AnyAsync(t => t.CarryOverReportId == reportId && t.TaskBarcode == barcode && !t.IsDeleted);
        }

        public async Task<int> GetNextItemNumberAsync(Guid reportId)
        {
            _logger.LogDebug("Getting next item number for report: {ReportId}", reportId);
            
            var maxItemNumber = await _context.CarryOverTasks
                .Where(t => t.CarryOverReportId == reportId && !t.IsDeleted)
                .MaxAsync(t => (int?)t.ItemNumber) ?? 0;
            
            return maxItemNumber + 1;
        }

        public async Task BulkCreateAsync(IEnumerable<CarryOverTask> tasks)
        {
            _logger.LogDebug("Bulk creating {Count} carry-over tasks", tasks.Count());
            
            await _context.CarryOverTasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Bulk created {Count} carry-over tasks", tasks.Count());
        }

        public async Task BulkUpdateAsync(IEnumerable<CarryOverTask> tasks)
        {
            _logger.LogDebug("Bulk updating {Count} carry-over tasks", tasks.Count());
            
            _context.CarryOverTasks.UpdateRange(tasks);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Bulk updated {Count} carry-over tasks", tasks.Count());
        }

        // Helper method for GetAllAsync used by statistics service
        public async Task<IEnumerable<CarryOverTask>> GetAllAsync()
        {
            _logger.LogDebug("Getting all carry-over tasks");
            
            return await _context.CarryOverTasks
                .Include(t => t.Report)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
