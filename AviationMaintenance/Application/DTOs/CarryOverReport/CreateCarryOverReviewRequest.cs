using System;
using System.ComponentModel.DataAnnotations;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class CreateCarryOverReviewRequest
    {
        [Required]
        public Guid CarryOverReportId { get; set; }

        [Required]
        public ReviewRole ReviewerRole { get; set; }

        [Required]
        public ReviewAction Action { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }
}
