using System;
using System.Collections.Generic;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class CarryOverReportResponse
    {
        public Guid Id { get; set; }
        public string ReportNumber { get; set; }
        public DateOnly ReportDate { get; set; }
        public string AircraftRegistration { get; set; }
        public string AircraftType { get; set; }
        public string FleetType { get; set; }
        public string WorkPackageDescription { get; set; }
        public DateTime InductionDate { get; set; }
        public DateTime PlannedDehangaringDate { get; set; }
        public DateTime? ActualDehangaringDate { get; set; }
        public string MaintenanceLocation { get; set; }
        public string CreatedByUserName { get; set; }
        public string SectionName { get; set; }
        public CarryOverReportStatus Status { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CarryOverTasks { get; set; }
        public decimal CarryOverPercentage { get; set; }
        public bool IsFinalized { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<CarryOverTaskResponse> Tasks { get; set; }
        public List<CarryOverReviewResponse> Reviews { get; set; }

        // Computed properties
        public TimeSpan TotalGroundTime => ActualDehangaringDate.HasValue 
            ? ActualDehangaringDate.Value - InductionDate 
            : PlannedDehangaringDate - InductionDate;

        public TimeSpan? DelayHours => ActualDehangaringDate.HasValue && ActualDehangaringDate.Value > PlannedDehangaringDate
            ? ActualDehangaringDate.Value - PlannedDehangaringDate
            : null;

        public string StatusDescription => Status.ToString();
    }
}
