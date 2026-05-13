using System;
using System.Collections.Generic;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    // DTOs for statistics and analytics
    public class CarryOverStatisticsDto
    {
        public int TotalReports { get; set; }
        public int TotalTasks { get; set; }
        public int TotalCarryOverTasks { get; set; }
        public decimal AverageCarryOverPercentage { get; set; }
        public int DelayedTasks { get; set; }
        public int RepeatTasks { get; set; }
    }

    public class FleetStatisticsDto
    {
        public string FleetType { get; set; }
        public int ReportsCount { get; set; }
        public int CarryOverTasks { get; set; }
        public decimal CarryOverPercentage { get; set; }
        public decimal AverageDelayHours { get; set; }
    }

    public class DeferralReasonStatisticsDto
    {
        public CarryOverDeferralReason Reason { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string ReasonDescription { get; set; }
    }

    public class TaskTypeStatisticsDto
    {
        public CarryOverTaskType TaskType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string TaskTypeDescription { get; set; }
    }

    public class AircraftStatisticsDto
    {
        public string AircraftRegistration { get; set; }
        public string AircraftType { get; set; }
        public int ReportsCount { get; set; }
        public int CarryOverTasks { get; set; }
        public decimal CarryOverPercentage { get; set; }
        public decimal TotalDelayHours { get; set; }
    }

    public class WeeklyTrendDto
    {
        public List<WeeklyDataPoint> DataPoints { get; set; } = new();
    }

    public class WeeklyDataPoint
    {
        public DateOnly WeekStart { get; set; }
        public DateOnly WeekEnd { get; set; }
        public int CarryOverTasks { get; set; }
        public decimal CarryOverPercentage { get; set; }
        public int ReportsCount { get; set; }
    }

    public class MonthlyTrendDto
    {
        public List<MonthlyDataPoint> DataPoints { get; set; } = new();
    }

    public class MonthlyDataPoint
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int CarryOverTasks { get; set; }
        public decimal CarryOverPercentage { get; set; }
        public int ReportsCount { get; set; }
    }

    public class RepeatTaskDto
    {
        public string TaskName { get; set; }
        public string TaskBarcode { get; set; }
        public int OccurrenceCount { get; set; }
        public List<string> AircraftRegistrations { get; set; } = new();
        public List<DateOnly> OccurrenceDates { get; set; } = new();
    }

    public class DelayImpactDto
    {
        public string AircraftRegistration { get; set; }
        public decimal TotalDelayHours { get; set; }
        public int DelayedTasks { get; set; }
        public decimal AverageDelayPerTask { get; set; }
        public List<string> TopDelayReasons { get; set; } = new();
    }
}
