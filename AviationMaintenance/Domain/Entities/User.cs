using System;
using System.Collections.Generic;

namespace AviationMaintenance.Domain.Entities
{
    /// <summary>
    /// Represents a system user with roles and organizational assignment
    /// </summary>
    public class User : AuditableEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Director, Manager, Team Leader, Employee, SAFA Inspector
        public Guid? SectionId { get; set; }
        public Guid? HangarId { get; set; }
        public Guid? ShopId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Section? Section { get; set; }
        public virtual Hangar? Hangar { get; set; }
        public virtual Shop? Shop { get; set; }
        public virtual ICollection<Bulletin> CreatedBulletins { get; set; } = new List<Bulletin>();
        public virtual ICollection<BulletinReadStatus> BulletinReadStatuses { get; set; } = new List<BulletinReadStatus>();
    }
}
