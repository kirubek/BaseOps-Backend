using System;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for listing bulletins with filters and pagination
    /// </summary>
    public class BulletinListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public BulletinPriority? Priority { get; set; }
        public BulletinCategory? Category { get; set; }
        public BulletinScope? Scope { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HangarId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
    }
}
