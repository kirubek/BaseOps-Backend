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
    /// Repository implementation for CarryOverReport operations
    /// </summary>
    public class CarryOverReportRepository : ICarryOverReportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CarryOverReportRepository> _logger;

        public CarryOverReportRepository(ApplicationDbContext context, ILogger<CarryOverReportRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarryOverReport?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Getting carry-over report by ID: {Id}", id);
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<CarryOverReport>> GetAllAsync()
        {
            _logger.LogDebug("Getting all carry-over reports");
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverReport>> GetBySectionAsync(Guid sectionId)
        {
            _logger.LogDebug("Getting carry-over reports by section ID: {SectionId}", sectionId);
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .Where(r => r.SectionId == sectionId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverReport>> GetByAircraftAsync(string aircraftRegistration)
        {
            _logger.LogDebug("Getting carry-over reports by aircraft registration: {AircraftRegistration}", aircraftRegistration);
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .Where(r => r.AircraftRegistration == aircraftRegistration)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverReport>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            _logger.LogDebug("Getting carry-over reports from {StartDate} to {EndDate}", startDate, endDate);
            
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .Where(r => r.ReportDate >= startDate && r.ReportDate <= endDate)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverReport>> GetByStatusAsync(CarryOverReportStatus status)
        {
            _logger.LogDebug("Getting carry-over reports by status: {Status}", status);
            
            return await _context.CarryOverReports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Section)
                .Include(r => r.Tasks)
                .Include(r => r.Reviews)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<CarryOverReport> CreateAsync(CarryOverReport report)
        {
            _logger.LogDebug("Creating carry-over report: {ReportNumber}", report.ReportNumber);
            
            await _context.CarryOverReports.AddAsync(report);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created carry-over report with ID: {Id}", report.Id);
            
            return report;
        }

        public async Task<CarryOverReport> UpdateAsync(CarryOverReport report)
        {
            _logger.LogDebug("Updating carry-over report: {Id}", report.Id);
            
            _context.CarryOverReports.Update(report);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated carry-over report with ID: {Id}", report.Id);
            
            return report;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting carry-over report: {Id}", id);
            
            var report = await _context.CarryOverReports.FindAsync(id);
            if (report == null)
                return false;

            // Soft delete
            report.IsDeleted = true;
            report.ModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted carry-over report with ID: {Id}", id);
            
            return true;
        }

        public async Task<int> CountAsync()
        {
            _logger.LogDebug("Counting all carry-over reports");
            
            return await _context.CarryOverReports.CountAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogDebug("Checking if carry-over report exists: {Id}", id);
            
            return await _context.CarryOverReports.AnyAsync(r => r.Id == id);
        }
    }
}
