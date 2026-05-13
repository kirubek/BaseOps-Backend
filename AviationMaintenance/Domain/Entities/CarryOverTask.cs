using System;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Common;

namespace AviationMaintenance.Domain.Entities
{
    public class CarryOverTask : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid CarryOverReportId { get; set; }

        public int ItemNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string TaskName { get; set; }

        [Required]
        [StringLength(100)]
        public string TaskBarcode { get; set; }

        public CarryOverTaskType TaskType { get; set; }

        public CarryOverDeferralReason DeferralReason { get; set; }

        [StringLength(500)]
        public string? CustomDeferralReason { get; set; }

        [StringLength(100)]
        public string? StockItem { get; set; }

        [StringLength(100)]
        public string? PartNumber { get; set; }

        [StringLength(50)]
        public string? StockId { get; set; }

        public CarryOverTaskOrigin DeferredTaskOrigin { get; set; }

        public InductionTiming Timing { get; set; }

        [StringLength(1000)]
        public string? Remark { get; set; }

        public bool AircraftDelayImpact { get; set; }

        [StringLength(500)]
        public string? DelayComment { get; set; }

        public bool IsRepeatTask { get; set; }

        public bool RequiresPlanningReview { get; set; }

        public bool RequiresManagementAttention { get; set; }

        public DateTime? DeferredUntilDate { get; set; }

        public virtual CarryOverReport Report { get; set; }
    }
}
