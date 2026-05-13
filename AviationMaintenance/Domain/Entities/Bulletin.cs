using System;
using System.Collections.Generic;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Domain.Entities
{
    /// <summary>
    /// Represents a digital operational notice board for aviation maintenance operations
    /// </summary>
    public class Bulletin : AuditableEntity
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The title of the bulletin
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The main content of the bulletin
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Priority level of the bulletin
        /// </summary>
        public BulletinPriority Priority { get; set; }

        /// <summary>
        /// Category of the bulletin
        /// </summary>
        public BulletinCategory Category { get; set; }

        /// <summary>
        /// Scope of the bulletin (who can see it)
        /// </summary>
        public BulletinScope Scope { get; set; }

        /// <summary>
        /// User who created the bulletin
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Section ID for section-scoped bulletins
        /// </summary>
        public Guid? SectionId { get; set; }

        /// <summary>
        /// Hangar ID for hangar-scoped bulletins
        /// </summary>
        public Guid? HangarId { get; set; }

        /// <summary>
        /// Shop ID for shop-scoped bulletins
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// When the bulletin was published
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// When the bulletin expires
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Whether the bulletin is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether the bulletin has expired
        /// </summary>
        public bool IsExpired { get; set; } = false;

        /// <summary>
        /// Whether the bulletin is soft-deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Section? Section { get; set; }
        public virtual Hangar? Hangar { get; set; }
        public virtual Shop? Shop { get; set; }
        public virtual ICollection<BulletinAttachment> Attachments { get; set; } = new List<BulletinAttachment>();
        public virtual ICollection<BulletinReadStatus> ReadStatuses { get; set; } = new List<BulletinReadStatus>();
    }
}
