using System;

namespace AviationMaintenance.Domain.Common
{
    /// <summary>
    /// Base class for auditable entities with timestamp and user tracking
    /// </summary>
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
