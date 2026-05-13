using System;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class UpdateCarryOverReportRequest
    {
        public Guid Id { get; set; }

        [StringLength(20)]
        public string? AircraftRegistration { get; set; }

        [StringLength(50)]
        public string? AircraftType { get; set; }

        [StringLength(50)]
        public string? FleetType { get; set; }

        [StringLength(500)]
        public string? WorkPackageDescription { get; set; }

        public DateTime? InductionDate { get; set; }

        public DateTime? PlannedDehangaringDate { get; set; }

        public DateTime? ActualDehangaringDate { get; set; }

        [StringLength(100)]
        public string? MaintenanceLocation { get; set; }

        public CarryOverReportStatus? Status { get; set; }
    }
}
