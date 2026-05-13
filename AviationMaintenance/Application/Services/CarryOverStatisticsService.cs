using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AviationMaintenance.Domain.Entities;
using AviationMaintenance.Domain.Enums;
using AviationMaintenance.Domain.Interfaces;

namespace AviationMaintenance.Application.Services
{
    /// <summary>
    /// Service for carry-over statistics and analytics
    /// </summary>
    public class CarryOverStatisticsService : ICarryOverStatisticsService
    {
        private readonly ICarryOverReportRepository _reportRepository;
        private readonly ICarryOverTaskRepository _taskRepository;
        private readonly ILogger<CarryOverStatisticsService> _logger;

        public CarryOverStatisticsService(
            ICarryOverReportRepository reportRepository,
            ICarryOverTaskRepository taskRepository,
            ILogger<CarryOverStatisticsService> logger)
        {
            _reportRepository = reportRepository;
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task<CarryOverStatisticsDto> GetOverallStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating overall statistics from {StartDate} to {EndDate}", startDate, endDate);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var allTasks = await GetAllTasksInDateRangeAsync(startDate, endDate);

            return new CarryOverStatisticsDto
            {
                TotalReports = reports.Count(),
                TotalTasks = allTasks.Count(),
                TotalCarryOverTasks = allTasks.Count(),
                AverageCarryOverPercentage = reports.Any() ? reports.Average(r => r.CarryOverPercentage) : 0,
                DelayedTasks = allTasks.Count(t => t.AircraftDelayImpact),
                RepeatTasks = allTasks.Count(t => t.IsRepeatTask)
            };
        }

        public async Task<IEnumerable<FleetStatisticsDto>> GetFleetStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating fleet statistics from {StartDate} to {EndDate}", startDate, endDate);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var fleetGroups = reports.GroupBy(r => r.FleetType);

            var result = new List<FleetStatisticsDto>();

            foreach (var group in fleetGroups)
            {
                var fleetReports = group.ToList();
                var reportIds = fleetReports.Select(r => r.Id);
                var fleetTasks = await GetTasksByReportIdsAsync(reportIds);

                result.Add(new FleetStatisticsDto
                {
                    FleetType = group.Key,
                    ReportsCount = fleetReports.Count,
                    CarryOverTasks = fleetTasks.Count(),
                    CarryOverPercentage = fleetReports.Any() ? fleetReports.Average(r => r.CarryOverPercentage) : 0,
                    AverageDelayHours = CalculateAverageDelayHours(fleetReports)
                });
            }

            return result.OrderByDescending(f => f.CarryOverTasks);
        }

        public async Task<IEnumerable<DeferralReasonStatisticsDto>> GetDeferralReasonStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating deferral reason statistics from {StartDate} to {EndDate}", startDate, endDate);

            var allTasks = await GetAllTasksInDateRangeAsync(startDate, endDate);
            var reasonGroups = allTasks.GroupBy(t => t.DeferralReason);
            var totalTasks = allTasks.Count();

            return reasonGroups.Select(group => new DeferralReasonStatisticsDto
            {
                Reason = group.Key,
                Count = group.Count(),
                Percentage = totalTasks > 0 ? (decimal)group.Count() / totalTasks * 100 : 0,
                ReasonDescription = group.Key.ToString()
            }).OrderByDescending(r => r.Count);
        }

        public async Task<IEnumerable<TaskTypeStatisticsDto>> GetTaskTypeStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating task type statistics from {StartDate} to {EndDate}", startDate, endDate);

            var allTasks = await GetAllTasksInDateRangeAsync(startDate, endDate);
            var typeGroups = allTasks.GroupBy(t => t.TaskType);
            var totalTasks = allTasks.Count();

            return typeGroups.Select(group => new TaskTypeStatisticsDto
            {
                TaskType = group.Key,
                Count = group.Count(),
                Percentage = totalTasks > 0 ? (decimal)group.Count() / totalTasks * 100 : 0,
                TaskTypeDescription = group.Key.ToString()
            }).OrderByDescending(t => t.Count);
        }

        public async Task<IEnumerable<AircraftStatisticsDto>> GetAircraftStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating aircraft statistics from {StartDate} to {EndDate}", startDate, endDate);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var aircraftGroups = reports.GroupBy(r => r.AircraftRegistration);

            var result = new List<AircraftStatisticsDto>();

            foreach (var group in aircraftGroups)
            {
                var aircraftReports = group.ToList();
                var reportIds = aircraftReports.Select(r => r.Id);
                var aircraftTasks = await GetTasksByReportIdsAsync(reportIds);

                result.Add(new AircraftStatisticsDto
                {
                    AircraftRegistration = group.Key,
                    AircraftType = aircraftReports.First().AircraftType,
                    ReportsCount = aircraftReports.Count,
                    CarryOverTasks = aircraftTasks.Count(),
                    CarryOverPercentage = aircraftReports.Any() ? aircraftReports.Average(r => r.CarryOverPercentage) : 0,
                    TotalDelayHours = CalculateTotalDelayHours(aircraftReports)
                });
            }

            return result.OrderByDescending(a => a.CarryOverTasks);
        }

        public async Task<WeeklyTrendDto> GetWeeklyTrendsAsync(int weeks = 12)
        {
            _logger.LogInformation("Calculating weekly trends for {Weeks} weeks", weeks);

            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = endDate.AddDays(-weeks * 7);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var weeklyData = new List<WeeklyDataPoint>();

            for (int i = 0; i < weeks; i++)
            {
                var weekStart = startDate.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(6);
                var weekReports = reports.Where(r => r.ReportDate >= weekStart && r.ReportDate <= weekEnd).ToList();
                var weekReportIds = weekReports.Select(r => r.Id);
                var weekTasks = await GetTasksByReportIdsAsync(weekReportIds);

                weeklyData.Add(new WeeklyDataPoint
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    CarryOverTasks = weekTasks.Count(),
                    CarryOverPercentage = weekReports.Any() ? weekReports.Average(r => r.CarryOverPercentage) : 0,
                    ReportsCount = weekReports.Count
                });
            }

            return new WeeklyTrendDto { DataPoints = weeklyData };
        }

        public async Task<MonthlyTrendDto> GetMonthlyTrendsAsync(int months = 12)
        {
            _logger.LogInformation("Calculating monthly trends for {Months} months", months);

            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = endDate.AddMonths(-months);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var monthlyData = new List<MonthlyDataPoint>();

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthReports = reports.Where(r => r.ReportDate >= monthStart && r.ReportDate <= monthEnd).ToList();
                var monthReportIds = monthReports.Select(r => r.Id);
                var monthTasks = await GetTasksByReportIdsAsync(monthReportIds);

                monthlyData.Add(new MonthlyDataPoint
                {
                    Year = monthStart.Year,
                    Month = monthStart.Month,
                    CarryOverTasks = monthTasks.Count(),
                    CarryOverPercentage = monthReports.Any() ? monthReports.Average(r => r.CarryOverPercentage) : 0,
                    ReportsCount = monthReports.Count
                });
            }

            return new MonthlyTrendDto { DataPoints = monthlyData };
        }

        public async Task<IEnumerable<RepeatTaskDto>> GetRepeatTasksAsync(int threshold = 3)
        {
            _logger.LogInformation("Finding repeat tasks with threshold {Threshold}", threshold);

            var allTasks = await _taskRepository.GetAllAsync();
            var taskGroups = allTasks.GroupBy(t => new { t.TaskName, t.TaskBarcode });

            var repeatTasks = taskGroups
                .Where(g => g.Count() >= threshold)
                .Select(g => new RepeatTaskDto
                {
                    TaskName = g.Key.TaskName,
                    TaskBarcode = g.Key.TaskBarcode,
                    OccurrenceCount = g.Count(),
                    AircraftRegistrations = g.Select(t => t.Report.AircraftRegistration).Distinct().ToList(),
                    OccurrenceDates = g.Select(t => DateOnly.FromDateTime(t.CreatedAt)).OrderBy(d => d).ToList()
                })
                .OrderByDescending(r => r.OccurrenceCount);

            return repeatTasks;
        }

        public async Task<IEnumerable<DelayImpactDto>> GetDelayImpactAnalysisAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _logger.LogInformation("Calculating delay impact analysis from {StartDate} to {EndDate}", startDate, endDate);

            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var aircraftGroups = reports.GroupBy(r => r.AircraftRegistration);

            var result = new List<DelayImpactDto>();

            foreach (var group in aircraftGroups)
            {
                var aircraftReports = group.ToList();
                var reportIds = aircraftReports.Select(r => r.Id);
                var aircraftTasks = await GetTasksByReportIdsAsync(reportIds);
                var delayedTasks = aircraftTasks.Where(t => t.AircraftDelayImpact).ToList();

                if (delayedTasks.Any())
                {
                    var delayReasons = delayedTasks
                        .GroupBy(t => t.DeferralReason)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => g.Key.ToString())
                        .ToList();

                    result.Add(new DelayImpactDto
                    {
                        AircraftRegistration = group.Key,
                        TotalDelayHours = CalculateTotalDelayHours(aircraftReports),
                        DelayedTasks = delayedTasks.Count,
                        AverageDelayPerTask = delayedTasks.Any() ? CalculateTotalDelayHours(aircraftReports) / delayedTasks.Count : 0,
                        TopDelayReasons = delayReasons
                    });
                }
            }

            return result.OrderByDescending(d => d.TotalDelayHours);
        }

        private async Task<IEnumerable<CarryOverReport>> GetReportsInDateRangeAsync(DateOnly? startDate, DateOnly? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return await _reportRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            
            return await _reportRepository.GetAllAsync();
        }

        private async Task<IEnumerable<CarryOverTask>> GetAllTasksInDateRangeAsync(DateOnly? startDate, DateOnly? endDate)
        {
            var reports = await GetReportsInDateRangeAsync(startDate, endDate);
            var reportIds = reports.Select(r => r.Id);
            return await GetTasksByReportIdsAsync(reportIds);
        }

        private async Task<IEnumerable<CarryOverTask>> GetTasksByReportIdsAsync(IEnumerable<Guid> reportIds)
        {
            var tasks = new List<CarryOverTask>();
            foreach (var reportId in reportIds)
            {
                var reportTasks = await _taskRepository.GetByReportIdAsync(reportId);
                tasks.AddRange(reportTasks);
            }
            return tasks;
        }

        private decimal CalculateAverageDelayHours(IEnumerable<CarryOverReport> reports)
        {
            var delayHours = reports
                .Where(r => r.ActualDehangaringDate.HasValue && r.ActualDehangaringDate.Value > r.PlannedDehangaringDate)
                .Select(r => (decimal)(r.ActualDehangaringDate.Value - r.PlannedDehangaringDate).TotalHours);

            return delayHours.Any() ? delayHours.Average() : 0;
        }

        private decimal CalculateTotalDelayHours(IEnumerable<CarryOverReport> reports)
        {
            return reports
                .Where(r => r.ActualDehangaringDate.HasValue && r.ActualDehangaringDate.Value > r.PlannedDehangaringDate)
                .Sum(r => (decimal)(r.ActualDehangaringDate.Value - r.PlannedDehangaringDate).TotalHours);
        }
    }
}
