using System;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for updating an existing bulletin
    /// </summary>
    public class UpdateBulletinRequest
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public BulletinPriority? Priority { get; set; }
        public BulletinCategory? Category { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
