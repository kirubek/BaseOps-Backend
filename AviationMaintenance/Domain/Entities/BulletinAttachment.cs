using System;

namespace AviationMaintenance.Domain.Entities
{
    /// <summary>
    /// Represents an attachment for a bulletin
    /// </summary>
    public class BulletinAttachment
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The bulletin this attachment belongs to
        /// </summary>
        public Guid BulletinId { get; set; }

        /// <summary>
        /// Original file name as uploaded by user
        /// </summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// Secure generated file name for storage
        /// </summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME content type of the file
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Relative file path for secure storage
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// User who uploaded the attachment
        /// </summary>
        public Guid UploadedByUserId { get; set; }

        /// <summary>
        /// When the attachment was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; }

        // Navigation properties
        public virtual Bulletin Bulletin { get; set; } = null!;
        public virtual User UploadedByUser { get; set; } = null!;
    }
}
