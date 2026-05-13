using System;
using System.Collections.Generic;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for bulletin analytics
    /// </summary>
    public class UserReadStatus
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string? SectionName { get; set; }
        public string? HangarName { get; set; }
        public string? ShopName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class BulletinAnalyticsResponse
    {
        public Guid BulletinId { get; set; }
        public string BulletinTitle { get; set; } = string.Empty;
        public int TotalRecipients { get; set; }
        public int ReadCount { get; set; }
        public int UnreadCount { get; set; }
        public List<UserReadStatus> UserReadStatuses { get; set; } = new List<UserReadStatus>();
        public DateTime PublishedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
