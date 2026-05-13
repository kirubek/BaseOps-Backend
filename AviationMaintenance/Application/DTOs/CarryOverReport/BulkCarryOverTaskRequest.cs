using System.Collections.Generic;
using AviationMaintenance.Domain.Enums;

namespace AviationMaintenance.Application.DTOs.CarryOverReport
{
    public class BulkCarryOverTaskRequest
    {
        public List<CreateCarryOverTaskRequest> Tasks { get; set; } = new();
    }
}
