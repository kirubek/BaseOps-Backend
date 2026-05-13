using System;
using System.Collections.Generic;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for bulletin response
    /// </summary>
    public class BulletinResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public BulletinPriority Priority { get; set; }
        public BulletinCategory Category { get; set; }
        public BulletinScope Scope { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string CreatedByUserRole { get; set; } = string.Empty;
        public Guid? SectionId { get; set; }
        public string? SectionName { get; set; }
        public Guid? HangarId { get; set; }
        public string? HangarName { get; set; }
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int AttachmentCount { get; set; }
        public int TotalRecipients { get; set; }
        public int ReadCount { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
