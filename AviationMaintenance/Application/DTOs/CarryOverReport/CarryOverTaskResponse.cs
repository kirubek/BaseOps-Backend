using System;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class CarryOverTaskResponse
    {
        public Guid Id { get; set; }
        public int ItemNumber { get; set; }
        public string TaskName { get; set; }
        public string TaskBarcode { get; set; }
        public CarryOverTaskType TaskType { get; set; }
        public CarryOverDeferralReason DeferralReason { get; set; }
        public string? CustomDeferralReason { get; set; }
        public string? StockItem { get; set; }
        public string? PartNumber { get; set; }
        public string? StockId { get; set; }
        public CarryOverTaskOrigin DeferredTaskOrigin { get; set; }
        public InductionTiming Timing { get; set; }
        public string? Remark { get; set; }
        public bool AircraftDelayImpact { get; set; }
        public string? DelayComment { get; set; }
        public bool IsRepeatTask { get; set; }
        public bool RequiresPlanningReview { get; set; }
        public bool RequiresManagementAttention { get; set; }
        public DateTime? DeferredUntilDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Computed properties
        public string TaskTypeDescription => TaskType.ToString();
        public string DeferralReasonDescription => DeferralReason.ToString();
        public string TimingDescription => Timing.ToString();
        public string DeferredTaskOriginDescription => DeferredTaskOrigin.ToString();
    }
}
