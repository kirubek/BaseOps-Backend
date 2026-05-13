using System;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class CarryOverReviewResponse
    {
        public Guid Id { get; set; }
        public Guid CarryOverReportId { get; set; }
        public Guid ReviewerUserId { get; set; }
        public string ReviewerUserName { get; set; }
        public ReviewRole ReviewerRole { get; set; }
        public ReviewAction Action { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewedAt { get; set; }

        // Computed properties
        public string ReviewerRoleDescription => ReviewerRole.ToString();
        public string ActionDescription => Action.ToString();
    }
}
