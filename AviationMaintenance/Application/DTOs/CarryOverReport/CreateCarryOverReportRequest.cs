using System;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class CreateCarryOverReportRequest
    {
        [Required]
        [StringLength(20)]
        public string AircraftRegistration { get; set; }

        [Required]
        [StringLength(50)]
        public string AircraftType { get; set; }

        [Required]
        [StringLength(50)]
        public string FleetType { get; set; }

        [Required]
        [StringLength(500)]
        public string WorkPackageDescription { get; set; }

        [Required]
        public DateTime InductionDate { get; set; }

        [Required]
        public DateTime PlannedDehangaringDate { get; set; }

        [Required]
        [StringLength(100)]
        public string MaintenanceLocation { get; set; }

        public Guid SectionId { get; set; }
    }
}
