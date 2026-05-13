using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AviationMaintenance.Application.DTOs.CarryOverReport;
using AviationMaintenance.Domain.Entities;
using AviationMaintenance.Domain.Enums;
using AviationMaintenance.Domain.Interfaces;

namespace AviationMaintenance.Application.Services
{
    /// <summary>
    /// Service for managing carry-over reports and related operations
    /// </summary>
    public class CarryOverReportService : ICarryOverReportService
    {
        private readonly ICarryOverReportRepository _reportRepository;
        private readonly ICarryOverTaskRepository _taskRepository;
        private readonly ICarryOverReviewRepository _reviewRepository;
        private readonly ILogger<CarryOverReportService> _logger;

        public CarryOverReportService(
            ICarryOverReportRepository reportRepository,
            ICarryOverTaskRepository taskRepository,
            ICarryOverReviewRepository reviewRepository,
            ILogger<CarryOverReportService> logger)
        {
            _reportRepository = reportRepository;
            _taskRepository = taskRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<CarryOverReportResponse> CreateReportAsync(CreateCarryOverReportRequest request, Guid userId)
        {
            _logger.LogInformation("Creating carry-over report for aircraft {Aircraft} by user {UserId}", 
                request.AircraftRegistration, userId);

            var report = new CarryOverReport
            {
                Id = Guid.NewGuid(),
                ReportNumber = await GenerateReportNumberAsync(),
                ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AircraftRegistration = request.AircraftRegistration,
                AircraftType = request.AircraftType,
                FleetType = request.FleetType,
                WorkPackageDescription = request.WorkPackageDescription,
                InductionDate = request.InductionDate,
                PlannedDehangaringDate = request.PlannedDehangaringDate,
                MaintenanceLocation = request.MaintenanceLocation,
                CreatedByUserId = userId,
                SectionId = request.SectionId,
                Status = CarryOverReportStatus.Draft,
                TotalTasks = 0,
                CompletedTasks = 0,
                CarryOverTasks = 0,
                CarryOverPercentage = 0,
                IsFinalized = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var createdReport = await _reportRepository.CreateAsync(report);
            
            _logger.LogInformation("Successfully created carry-over report {ReportId} with number {ReportNumber}", 
                createdReport.Id, createdReport.ReportNumber);

            return await MapToResponseAsync(createdReport);
        }

        public async Task<CarryOverReportResponse?> GetReportByIdAsync(Guid id)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            return report != null ? await MapToResponseAsync(report) : null;
        }

        public async Task<IEnumerable<CarryOverReportResponse>> GetAllReportsAsync()
        {
            var reports = await _reportRepository.GetAllAsync();
            return await Task.WhenAll(reports.Select(MapToResponseAsync));
        }

        public async Task<IEnumerable<CarryOverReportResponse>> GetReportsBySectionAsync(Guid sectionId)
        {
            var reports = await _reportRepository.GetBySectionAsync(sectionId);
            return await Task.WhenAll(reports.Select(MapToResponseAsync));
        }

        public async Task<IEnumerable<CarryOverReportResponse>> GetReportsByAircraftAsync(string aircraftRegistration)
        {
            var reports = await _reportRepository.GetByAircraftAsync(aircraftRegistration);
            return await Task.WhenAll(reports.Select(MapToResponseAsync));
        }

        public async Task<CarryOverReportResponse> UpdateReportAsync(UpdateCarryOverReportRequest request, Guid userId)
        {
            var existingReport = await _reportRepository.GetByIdAsync(request.Id);
            if (existingReport == null)
                throw new KeyNotFoundException($"Report with ID {request.Id} not found");

            if (existingReport.IsFinalized)
                throw new InvalidOperationException("Cannot update a finalized report");

            // Update properties
            if (request.AircraftRegistration != null) existingReport.AircraftRegistration = request.AircraftRegistration;
            if (request.AircraftType != null) existingReport.AircraftType = request.AircraftType;
            if (request.FleetType != null) existingReport.FleetType = request.FleetType;
            if (request.WorkPackageDescription != null) existingReport.WorkPackageDescription = request.WorkPackageDescription;
            if (request.InductionDate.HasValue) existingReport.InductionDate = request.InductionDate.Value;
            if (request.PlannedDehangaringDate.HasValue) existingReport.PlannedDehangaringDate = request.PlannedDehangaringDate.Value;
            if (request.ActualDehangaringDate.HasValue) existingReport.ActualDehangaringDate = request.ActualDehangaringDate.Value;
            if (request.MaintenanceLocation != null) existingReport.MaintenanceLocation = request.MaintenanceLocation;
            if (request.Status.HasValue) existingReport.Status = request.Status.Value;

            existingReport.ModifiedAt = DateTime.UtcNow;
            existingReport.ModifiedBy = userId;

            var updatedReport = await _reportRepository.UpdateAsync(existingReport);
            
            _logger.LogInformation("Updated carry-over report {ReportId}", updatedReport.Id);

            return await MapToResponseAsync(updatedReport);
        }

        public async Task<bool> DeleteReportAsync(Guid id)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
                return false;

            if (report.IsFinalized)
                throw new InvalidOperationException("Cannot delete a finalized report");

            // Delete related tasks and reviews first
            var tasks = await _taskRepository.GetByReportIdAsync(id);
            foreach (var task in tasks)
            {
                await _taskRepository.DeleteAsync(task.Id);
            }

            var reviews = await _reviewRepository.GetByReportIdAsync(id);
            foreach (var review in reviews)
            {
                await _reviewRepository.DeleteAsync(review.Id);
            }

            var result = await _reportRepository.DeleteAsync(id);
            
            _logger.LogInformation("Deleted carry-over report {ReportId}", id);

            return result;
        }

        public async Task<CarryOverReportResponse> SubmitReportAsync(Guid id, Guid userId)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {id} not found");

            if (report.Status != CarryOverReportStatus.Draft)
                throw new InvalidOperationException("Only draft reports can be submitted");

            // Calculate final statistics
            await CalculateReportStatisticsAsync(report);

            report.Status = CarryOverReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.ModifiedAt = DateTime.UtcNow;
            report.ModifiedBy = userId;

            var updatedReport = await _reportRepository.UpdateAsync(report);
            
            _logger.LogInformation("Submitted carry-over report {ReportId}", id);

            return await MapToResponseAsync(updatedReport);
        }

        public async Task<CarryOverReportResponse> FinalizeReportAsync(Guid id, Guid userId)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {id} not found");

            if (report.Status != CarryOverReportStatus.Submitted && report.Status != CarryOverReportStatus.Reviewed)
                throw new InvalidOperationException("Only submitted or reviewed reports can be finalized");

            report.Status = CarryOverReportStatus.Finalized;
            report.IsFinalized = true;
            report.ModifiedAt = DateTime.UtcNow;
            report.ModifiedBy = userId;

            var updatedReport = await _reportRepository.UpdateAsync(report);
            
            _logger.LogInformation("Finalized carry-over report {ReportId}", id);

            return await MapToResponseAsync(updatedReport);
        }

        public async Task<CarryOverTaskResponse> AddTaskAsync(CreateCarryOverTaskRequest request, Guid userId)
        {
            var report = await _reportRepository.GetByIdAsync(request.CarryOverReportId);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {request.CarryOverReportId} not found");

            if (report.IsFinalized)
                throw new InvalidOperationException("Cannot add tasks to a finalized report");

            // Check for duplicate barcode within the report
            if (await _taskRepository.BarcodeExistsInReportAsync(request.CarryOverReportId, request.TaskBarcode))
                throw new InvalidOperationException($"Task barcode {request.TaskBarcode} already exists in this report");

            var task = new CarryOverTask
            {
                Id = Guid.NewGuid(),
                CarryOverReportId = request.CarryOverReportId,
                ItemNumber = await _taskRepository.GetNextItemNumberAsync(request.CarryOverReportId),
                TaskName = request.TaskName,
                TaskBarcode = request.TaskBarcode,
                TaskType = request.TaskType,
                DeferralReason = request.DeferralReason,
                CustomDeferralReason = request.CustomDeferralReason,
                StockItem = request.StockItem,
                PartNumber = request.PartNumber,
                StockId = request.StockId,
                DeferredTaskOrigin = request.DeferredTaskOrigin,
                Timing = request.Timing,
                Remark = request.Remark,
                AircraftDelayImpact = request.AircraftDelayImpact,
                DelayComment = request.DelayComment,
                IsRepeatTask = request.IsRepeatTask,
                RequiresPlanningReview = request.RequiresPlanningReview,
                RequiresManagementAttention = request.RequiresManagementAttention,
                DeferredUntilDate = request.DeferredUntilDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var createdTask = await _taskRepository.CreateAsync(task);
            
            // Update report statistics
            await CalculateReportStatisticsAsync(report);

            _logger.LogInformation("Added task {TaskId} to report {ReportId}", createdTask.Id, request.CarryOverReportId);

            return await MapTaskToResponseAsync(createdTask);
        }

        public async Task<CarryOverTaskResponse> UpdateTaskAsync(UpdateCarryOverTaskRequest request, Guid userId)
        {
            var existingTask = await _taskRepository.GetByIdAsync(request.Id);
            if (existingTask == null)
                throw new KeyNotFoundException($"Task with ID {request.Id} not found");

            var report = await _reportRepository.GetByIdAsync(existingTask.CarryOverReportId);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {existingTask.CarryOverReportId} not found");

            if (report.IsFinalized)
                throw new InvalidOperationException("Cannot update tasks in a finalized report");

            // Check for duplicate barcode if barcode is being changed
            if (request.TaskBarcode != null && request.TaskBarcode != existingTask.TaskBarcode)
            {
                if (await _taskRepository.BarcodeExistsInReportAsync(existingTask.CarryOverReportId, request.TaskBarcode))
                    throw new InvalidOperationException($"Task barcode {request.TaskBarcode} already exists in this report");
            }

            // Update properties
            if (request.TaskName != null) existingTask.TaskName = request.TaskName;
            if (request.TaskBarcode != null) existingTask.TaskBarcode = request.TaskBarcode;
            if (request.TaskType.HasValue) existingTask.TaskType = request.TaskType.Value;
            if (request.DeferralReason.HasValue) existingTask.DeferralReason = request.DeferralReason.Value;
            if (request.CustomDeferralReason != null) existingTask.CustomDeferralReason = request.CustomDeferralReason;
            if (request.StockItem != null) existingTask.StockItem = request.StockItem;
            if (request.PartNumber != null) existingTask.PartNumber = request.PartNumber;
            if (request.StockId != null) existingTask.StockId = request.StockId;
            if (request.DeferredTaskOrigin.HasValue) existingTask.DeferredTaskOrigin = request.DeferredTaskOrigin.Value;
            if (request.Timing.HasValue) existingTask.Timing = request.Timing.Value;
            if (request.Remark != null) existingTask.Remark = request.Remark;
            if (request.AircraftDelayImpact.HasValue) existingTask.AircraftDelayImpact = request.AircraftDelayImpact.Value;
            if (request.DelayComment != null) existingTask.DelayComment = request.DelayComment;
            if (request.IsRepeatTask.HasValue) existingTask.IsRepeatTask = request.IsRepeatTask.Value;
            if (request.RequiresPlanningReview.HasValue) existingTask.RequiresPlanningReview = request.RequiresPlanningReview.Value;
            if (request.RequiresManagementAttention.HasValue) existingTask.RequiresManagementAttention = request.RequiresManagementAttention.Value;
            if (request.DeferredUntilDate.HasValue) existingTask.DeferredUntilDate = request.DeferredUntilDate.Value;

            existingTask.ModifiedAt = DateTime.UtcNow;
            existingTask.ModifiedBy = userId;

            var updatedTask = await _taskRepository.UpdateAsync(existingTask);
            
            // Update report statistics
            await CalculateReportStatisticsAsync(report);

            _logger.LogInformation("Updated task {TaskId}", updatedTask.Id);

            return await MapTaskToResponseAsync(updatedTask);
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return false;

            var report = await _reportRepository.GetByIdAsync(task.CarryOverReportId);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {task.CarryOverReportId} not found");

            if (report.IsFinalized)
                throw new InvalidOperationException("Cannot delete tasks from a finalized report");

            var result = await _taskRepository.DeleteAsync(id);
            
            if (result)
            {
                // Update report statistics
                await CalculateReportStatisticsAsync(report);
                _logger.LogInformation("Deleted task {TaskId}", id);
            }

            return result;
        }

        public async Task<IEnumerable<CarryOverTaskResponse>> AddBulkTasksAsync(BulkCarryOverTaskRequest request, Guid userId)
        {
            var report = await _reportRepository.GetByIdAsync(request.Tasks.FirstOrDefault()?.CarryOverReportId ?? Guid.Empty);
            if (report == null)
                throw new KeyNotFoundException("Report not found for bulk task creation");

            if (report.IsFinalized)
                throw new InvalidOperationException("Cannot add tasks to a finalized report");

            var tasks = new List<CarryOverTask>();
            var itemNumber = await _taskRepository.GetNextItemNumberAsync(report.Id);

            foreach (var taskRequest in request.Tasks)
            {
                // Check for duplicate barcode
                if (await _taskRepository.BarcodeExistsInReportAsync(report.Id, taskRequest.TaskBarcode))
                    throw new InvalidOperationException($"Task barcode {taskRequest.TaskBarcode} already exists in this report");

                var task = new CarryOverTask
                {
                    Id = Guid.NewGuid(),
                    CarryOverReportId = taskRequest.CarryOverReportId,
                    ItemNumber = itemNumber++,
                    TaskName = taskRequest.TaskName,
                    TaskBarcode = taskRequest.TaskBarcode,
                    TaskType = taskRequest.TaskType,
                    DeferralReason = taskRequest.DeferralReason,
                    CustomDeferralReason = taskRequest.CustomDeferralReason,
                    StockItem = taskRequest.StockItem,
                    PartNumber = taskRequest.PartNumber,
                    StockId = taskRequest.StockId,
                    DeferredTaskOrigin = taskRequest.DeferredTaskOrigin,
                    Timing = taskRequest.Timing,
                    Remark = taskRequest.Remark,
                    AircraftDelayImpact = taskRequest.AircraftDelayImpact,
                    DelayComment = taskRequest.DelayComment,
                    IsRepeatTask = taskRequest.IsRepeatTask,
                    RequiresPlanningReview = taskRequest.RequiresPlanningReview,
                    RequiresManagementAttention = taskRequest.RequiresManagementAttention,
                    DeferredUntilDate = taskRequest.DeferredUntilDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                tasks.Add(task);
            }

            await _taskRepository.BulkCreateAsync(tasks);
            
            // Update report statistics
            await CalculateReportStatisticsAsync(report);

            _logger.LogInformation("Added {TaskCount} bulk tasks to report {ReportId}", tasks.Count, report.Id);

            return await Task.WhenAll(tasks.Select(MapTaskToResponseAsync));
        }

        public async Task<CarryOverReviewResponse> AddReviewAsync(CreateCarryOverReviewRequest request, Guid userId)
        {
            var report = await _reportRepository.GetByIdAsync(request.CarryOverReportId);
            if (report == null)
                throw new KeyNotFoundException($"Report with ID {request.CarryOverReportId} not found");

            if (report.Status == CarryOverReportStatus.Draft)
                throw new InvalidOperationException("Cannot review draft reports");

            var review = new CarryOverReview
            {
                Id = Guid.NewGuid(),
                CarryOverReportId = request.CarryOverReportId,
                ReviewerUserId = userId,
                ReviewerRole = request.ReviewerRole,
                Action = request.Action,
                Comment = request.Comment,
                ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            var createdReview = await _reviewRepository.CreateAsync(review);
            
            // Update report status if needed
            if (report.Status == CarryOverReportStatus.Submitted)
            {
                report.Status = CarryOverReportStatus.Reviewed;
                report.ModifiedAt = DateTime.UtcNow;
                report.ModifiedBy = userId;
                await _reportRepository.UpdateAsync(report);
            }

            _logger.LogInformation("Added review {ReviewId} to report {ReportId}", createdReview.Id, request.CarryOverReportId);

            return await MapReviewToResponseAsync(createdReview);
        }

        private async Task<string> GenerateReportNumberAsync()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _reportRepository.CountAsync() + 1;
            return $"COR-{date}-{count:D4}";
        }

        private async Task CalculateReportStatisticsAsync(CarryOverReport report)
        {
            var tasks = await _taskRepository.GetByReportIdAsync(report.Id);
            var tasksList = tasks.ToList();

            report.TotalTasks = tasksList.Count;
            report.CarryOverTasks = tasksList.Count; // All tasks in carry-over report are carry-over tasks
            report.CompletedTasks = 0; // This would be calculated based on actual completion status
            report.CarryOverPercentage = report.TotalTasks > 0 
                ? (decimal)report.CarryOverTasks / report.TotalTasks * 100 
                : 0;

            await _reportRepository.UpdateAsync(report);
        }

        private async Task<CarryOverReportResponse> MapToResponseAsync(CarryOverReport report)
        {
            var tasks = await _taskRepository.GetByReportIdAsync(report.Id);
            var reviews = await _reviewRepository.GetByReportIdAsync(report.Id);

            return new CarryOverReportResponse
            {
                Id = report.Id,
                ReportNumber = report.ReportNumber,
                ReportDate = report.ReportDate,
                AircraftRegistration = report.AircraftRegistration,
                AircraftType = report.AircraftType,
                FleetType = report.FleetType,
                WorkPackageDescription = report.WorkPackageDescription,
                InductionDate = report.InductionDate,
                PlannedDehangaringDate = report.PlannedDehangaringDate,
                ActualDehangaringDate = report.ActualDehangaringDate,
                MaintenanceLocation = report.MaintenanceLocation,
                CreatedByUserName = report.CreatedByUser?.FullName ?? "Unknown",
                SectionName = report.Section?.Name ?? "Unknown",
                Status = report.Status,
                TotalTasks = report.TotalTasks,
                CompletedTasks = report.CompletedTasks,
                CarryOverTasks = report.CarryOverTasks,
                CarryOverPercentage = report.CarryOverPercentage,
                IsFinalized = report.IsFinalized,
                SubmittedAt = report.SubmittedAt,
                CreatedAt = report.CreatedAt,
                ModifiedAt = report.ModifiedAt,
                Tasks = await Task.WhenAll(tasks.Select(MapTaskToResponseAsync)),
                Reviews = await Task.WhenAll(reviews.Select(MapReviewToResponseAsync))
            };
        }

        private async Task<CarryOverTaskResponse> MapTaskToResponseAsync(CarryOverTask task)
        {
            return new CarryOverTaskResponse
            {
                Id = task.Id,
                ItemNumber = task.ItemNumber,
                TaskName = task.TaskName,
                TaskBarcode = task.TaskBarcode,
                TaskType = task.TaskType,
                DeferralReason = task.DeferralReason,
                CustomDeferralReason = task.CustomDeferralReason,
                StockItem = task.StockItem,
                PartNumber = task.PartNumber,
                StockId = task.StockId,
                DeferredTaskOrigin = task.DeferredTaskOrigin,
                Timing = task.Timing,
                Remark = task.Remark,
                AircraftDelayImpact = task.AircraftDelayImpact,
                DelayComment = task.DelayComment,
                IsRepeatTask = task.IsRepeatTask,
                RequiresPlanningReview = task.RequiresPlanningReview,
                RequiresManagementAttention = task.RequiresManagementAttention,
                DeferredUntilDate = task.DeferredUntilDate,
                CreatedAt = task.CreatedAt,
                ModifiedAt = task.ModifiedAt
            };
        }

        private async Task<CarryOverReviewResponse> MapReviewToResponseAsync(CarryOverReview review)
        {
            return new CarryOverReviewResponse
            {
                Id = review.Id,
                CarryOverReportId = review.CarryOverReportId,
                ReviewerUserId = review.ReviewerUserId,
                ReviewerUserName = review.ReviewerUser?.FullName ?? "Unknown",
                ReviewerRole = review.ReviewerRole,
                Action = review.Action,
                Comment = review.Comment,
                ReviewedAt = review.ReviewedAt
            };
        }
    }

    public interface ICarryOverReportService
    {
        Task<CarryOverReportResponse> CreateReportAsync(CreateCarryOverReportRequest request, Guid userId);
        Task<CarryOverReportResponse?> GetReportByIdAsync(Guid id);
        Task<IEnumerable<CarryOverReportResponse>> GetAllReportsAsync();
        Task<IEnumerable<CarryOverReportResponse>> GetReportsBySectionAsync(Guid sectionId);
        Task<IEnumerable<CarryOverReportResponse>> GetReportsByAircraftAsync(string aircraftRegistration);
        Task<CarryOverReportResponse> UpdateReportAsync(UpdateCarryOverReportRequest request, Guid userId);
        Task<bool> DeleteReportAsync(Guid id);
        Task<CarryOverReportResponse> SubmitReportAsync(Guid id, Guid userId);
        Task<CarryOverReportResponse> FinalizeReportAsync(Guid id, Guid userId);
        Task<CarryOverTaskResponse> AddTaskAsync(CreateCarryOverTaskRequest request, Guid userId);
        Task<CarryOverTaskResponse> UpdateTaskAsync(UpdateCarryOverTaskRequest request, Guid userId);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<IEnumerable<CarryOverTaskResponse>> AddBulkTasksAsync(BulkCarryOverTaskRequest request, Guid userId);
        Task<CarryOverReviewResponse> AddReviewAsync(CreateCarryOverReviewRequest request, Guid userId);
    }
}
