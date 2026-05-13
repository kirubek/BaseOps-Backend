using System;
using System.Collections.Generic;

namespace AviationMaintenance.Application.DTOs.Bulletin
{
    /// <summary>
    /// DTO for paginated bulletin list response
    /// </summary>
    public class BulletinListResponse
    {
        public List<BulletinResponse> Bulletins { get; set; } = new List<BulletinResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int UnreadCount { get; set; }
        public int HighPriorityUnreadCount { get; set; }
    }
}
