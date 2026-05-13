using System;
using System.Threading.Tasks;

namespace AviationMaintenance.Application.Interfaces
{
    /// <summary>
    /// Audit logging interface for tracking system operations
    /// </summary>
    public class AuditEvent
    {
        public string Action { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public interface IAuditLogger
    {
        Task LogAsync(AuditEvent auditEvent);
    }
}
