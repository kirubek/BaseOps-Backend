using System;
using System.Collections.Generic;

namespace AviationMaintenance.Domain.Entities
{
    /// <summary>
    /// Represents a maintenance section within the organization
    /// </summary>
    public class Section : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
    }
}
