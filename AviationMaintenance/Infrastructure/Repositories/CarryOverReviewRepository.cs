using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AviationMaintenance.Domain.Entities;
using AviationMaintenance.Domain.Interfaces;
using AviationMaintenance.Infrastructure.Data;

namespace AviationMaintenance.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for CarryOverReview operations
    /// </summary>
    public class CarryOverReviewRepository : ICarryOverReviewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CarryOverReviewRepository> _logger;

        public CarryOverReviewRepository(ApplicationDbContext context, ILogger<CarryOverReviewRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarryOverReview?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Getting carry-over review by ID: {Id}", id);
            
            return await _context.CarryOverReviews
                .Include(r => r.CarryOverReport)
                .Include(r => r.ReviewerUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<CarryOverReview>> GetByReportIdAsync(Guid reportId)
        {
            _logger.LogDebug("Getting carry-over reviews by report ID: {ReportId}", reportId);
            
            return await _context.CarryOverReviews
                .Include(r => r.CarryOverReport)
                .Include(r => r.ReviewerUser)
                .Where(r => r.CarryOverReportId == reportId)
                .OrderByDescending(r => r.ReviewedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarryOverReview>> GetByReviewerIdAsync(Guid reviewerId)
        {
            _logger.LogDebug("Getting carry-over reviews by reviewer ID: {ReviewerId}", reviewerId);
            
            return await _context.CarryOverReviews
                .Include(r => r.CarryOverReport)
                .Include(r => r.ReviewerUser)
                .Where(r => r.ReviewerUserId == reviewerId)
                .OrderByDescending(r => r.ReviewedAt)
                .ToListAsync();
        }

        public async Task<CarryOverReview> CreateAsync(CarryOverReview review)
        {
            _logger.LogDebug("Creating carry-over review for report: {ReportId}", review.CarryOverReportId);
            
            await _context.CarryOverReviews.AddAsync(review);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created carry-over review with ID: {Id}", review.Id);
            
            return review;
        }

        public async Task<CarryOverReview> UpdateAsync(CarryOverReview review)
        {
            _logger.LogDebug("Updating carry-over review: {Id}", review.Id);
            
            _context.CarryOverReviews.Update(review);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated carry-over review with ID: {Id}", review.Id);
            
            return review;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogDebug("Deleting carry-over review: {Id}", id);
            
            var review = await _context.CarryOverReviews.FindAsync(id);
            if (review == null)
                return false;

            // Soft delete
            review.IsDeleted = true;
            review.ModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted carry-over review with ID: {Id}", id);
            
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogDebug("Checking if carry-over review exists: {Id}", id);
            
            return await _context.CarryOverReviews.AnyAsync(r => r.Id == id);
        }
    }
}
