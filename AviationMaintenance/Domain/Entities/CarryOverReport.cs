using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Common;

namespace AviationMaintenance.Domain.Entities
{
    public class CarryOverReport : AuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ReportNumber { get; set; }

        public DateOnly ReportDate { get; set; }

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

        public DateTime InductionDate { get; set; }

        public DateTime PlannedDehangaringDate { get; set; }

        public DateTime? ActualDehangaringDate { get; set; }

        [Required]
        [StringLength(100)]
        public string MaintenanceLocation { get; set; }

        public Guid CreatedByUserId { get; set; }

        public Guid SectionId { get; set; }

        public CarryOverReportStatus Status { get; set; }

        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int CarryOverTasks { get; set; }

        public decimal CarryOverPercentage { get; set; }

        public bool IsFinalized { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public virtual ICollection<CarryOverTask> Tasks { get; set; }
        public virtual ICollection<CarryOverReview> Reviews { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual Section Section { get; set; }
    }
}
