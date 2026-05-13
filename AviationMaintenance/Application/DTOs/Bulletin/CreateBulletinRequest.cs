using System;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for creating a new bulletin
    /// </summary>
    public class CreateBulletinRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public BulletinPriority Priority { get; set; }
        public BulletinCategory Category { get; set; }
        public BulletinScope Scope { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? HangarId { get; set; }
        public Guid? ShopId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool PublishImmediately { get; set; }
    }
}
