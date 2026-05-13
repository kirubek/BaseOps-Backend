using FluentValidation;
using AviationMaintenance.Application.DTOs.CarryOverReport;

namespace AviationMaintenance.Application.Validators.CarryOverReport
{
    /// <summary>
    /// Validator for CreateCarryOverReportRequest
    /// </summary>
    public class CreateCarryOverReportRequestValidator : AbstractValidator<CreateCarryOverReportRequest>
    {
        public CreateCarryOverReportRequestValidator()
        {
            RuleFor(x => x.AircraftRegistration)
                .NotEmpty()
                .WithMessage("Aircraft registration is required")
                .MaximumLength(20)
                .WithMessage("Aircraft registration cannot exceed 20 characters")
                .Matches(@"^[A-Z]{2}-[A-Z0-9]{3,4}$")
                .WithMessage("Aircraft registration must be in format like ET-AXT or N12345");

            RuleFor(x => x.AircraftType)
                .NotEmpty()
                .WithMessage("Aircraft type is required")
                .MaximumLength(50)
                .WithMessage("Aircraft type cannot exceed 50 characters")
                .Must(BeValidAircraftType)
                .WithMessage("Invalid aircraft type. Must be one of: B737, B767, B777, B787, A350, Q400");

            RuleFor(x => x.FleetType)
                .NotEmpty()
                .WithMessage("Fleet type is required")
                .MaximumLength(50)
                .WithMessage("Fleet type cannot exceed 50 characters");

            RuleFor(x => x.WorkPackageDescription)
                .NotEmpty()
                .WithMessage("Work package description is required")
                .MaximumLength(500)
                .WithMessage("Work package description cannot exceed 500 characters");

            RuleFor(x => x.InductionDate)
                .NotEmpty()
                .WithMessage("Induction date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Induction date cannot be in the future");

            RuleFor(x => x.PlannedDehangaringDate)
                .NotEmpty()
                .WithMessage("Planned dehangaring date is required")
                .GreaterThan(x => x.InductionDate)
                .WithMessage("Planned dehangaring date must be after induction date");

            RuleFor(x => x.MaintenanceLocation)
                .NotEmpty()
                .WithMessage("Maintenance location is required")
                .MaximumLength(100)
                .WithMessage("Maintenance location cannot exceed 100 characters");

            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required");
        }

        private bool BeValidAircraftType(string aircraftType)
        {
            var validTypes = new[] { "B737", "B767", "B777", "B787", "A350", "Q400" };
            return validTypes.Contains(aircraftType.ToUpper());
        }
    }
}
