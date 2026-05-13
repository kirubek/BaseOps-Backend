using System;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Common;

namespace AviationMaintenance.Domain.Entities
{
    public class CarryOverReview : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid CarryOverReportId { get; set; }

        public Guid ReviewerUserId { get; set; }

        public ReviewRole ReviewerRole { get; set; }

        public ReviewAction Action { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewedAt { get; set; }

        // Navigation properties
        public virtual CarryOverReport CarryOverReport { get; set; }
        public virtual User ReviewerUser { get; set; }
    }
}
