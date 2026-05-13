using FluentValidation;
using AviationMaintenance.Application.DTOs.CarryOverReport;

namespace AviationMaintenance.Application.Validators.CarryOverReport
{
    /// <summary>
    /// Validator for UpdateCarryOverReportRequest
    /// </summary>
    public class UpdateCarryOverReportRequestValidator : AbstractValidator<UpdateCarryOverReportRequest>
    {
        public UpdateCarryOverReportRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Report ID is required");

            RuleFor(x => x.AircraftRegistration)
                .MaximumLength(20)
                .WithMessage("Aircraft registration cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.AircraftRegistration))
                .Matches(@"^[A-Z]{2}-[A-Z0-9]{3,4}$")
                .WithMessage("Aircraft registration must be in format like ET-AXT or N12345")
                .When(x => !string.IsNullOrEmpty(x.AircraftRegistration));

            RuleFor(x => x.AircraftType)
                .MaximumLength(50)
                .WithMessage("Aircraft type cannot exceed 50 characters")
                .Must(BeValidAircraftType)
                .WithMessage("Invalid aircraft type. Must be one of: B737, B767, B777, B787, A350, Q400")
                .When(x => !string.IsNullOrEmpty(x.AircraftType));

            RuleFor(x => x.FleetType)
                .MaximumLength(50)
                .WithMessage("Fleet type cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FleetType));

            RuleFor(x => x.WorkPackageDescription)
                .MaximumLength(500)
                .WithMessage("Work package description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.WorkPackageDescription));

            RuleFor(x => x.InductionDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Induction date cannot be in the future")
                .When(x => x.InductionDate.HasValue);

            RuleFor(x => x.PlannedDehangaringDate)
                .GreaterThan(x => x.InductionDate)
                .WithMessage("Planned dehangaring date must be after induction date")
                .When(x => x.PlannedDehangaringDate.HasValue && x.InductionDate.HasValue);

            RuleFor(x => x.ActualDehangaringDate)
                .GreaterThan(x => x.InductionDate)
                .WithMessage("Actual dehangaring date must be after induction date")
                .When(x => x.ActualDehangaringDate.HasValue && x.InductionDate.HasValue);

            RuleFor(x => x.MaintenanceLocation)
                .MaximumLength(100)
                .WithMessage("Maintenance location cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.MaintenanceLocation));
        }

        private bool BeValidAircraftType(string aircraftType)
        {
            var validTypes = new[] { "B737", "B767", "B777", "B787", "A350", "Q400" };
            return validTypes.Contains(aircraftType.ToUpper());
        }
    }
}
