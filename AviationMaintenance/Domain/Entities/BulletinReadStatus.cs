using System;

namespace AviationMaintenance.Domain.Entities
{
    /// <summary>
    /// Tracks read/unread status of bulletins per user
    /// </summary>
    public class BulletinReadStatus
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The bulletin this status belongs to
        /// </summary>
        public Guid BulletinId { get; set; }

        /// <summary>
        /// The user this status belongs to
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Whether the bulletin has been read by the user
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When the bulletin was marked as read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public virtual Bulletin Bulletin { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
