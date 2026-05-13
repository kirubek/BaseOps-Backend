using FluentValidation;
using AviationMaintenance.Application.DTOs.CarryOverReport;

namespace AviationMaintenance.Application.Validators.CarryOverReport
{
    /// <summary>
    /// Validator for CreateCarryOverTaskRequest
    /// </summary>
    public class CreateCarryOverTaskRequestValidator : AbstractValidator<CreateCarryOverTaskRequest>
    {
        public CreateCarryOverTaskRequestValidator()
        {
            RuleFor(x => x.CarryOverReportId)
                .NotEmpty()
                .WithMessage("Carry-over report ID is required");

            RuleFor(x => x.TaskName)
                .NotEmpty()
                .WithMessage("Task name is required")
                .MaximumLength(500)
                .WithMessage("Task name cannot exceed 500 characters");

            RuleFor(x => x.TaskBarcode)
                .NotEmpty()
                .WithMessage("Task barcode is required")
                .MaximumLength(100)
                .WithMessage("Task barcode cannot exceed 100 characters")
                .Matches(@"^[A-Z0-9\-\/]+$")
                .WithMessage("Task barcode can only contain uppercase letters, numbers, hyphens, and forward slashes");

            RuleFor(x => x.TaskType)
                .IsInEnum()
                .WithMessage("Invalid task type");

            RuleFor(x => x.DeferralReason)
                .IsInEnum()
                .WithMessage("Invalid deferral reason");

            RuleFor(x => x.CustomDeferralReason)
                .MaximumLength(500)
                .WithMessage("Custom deferral reason cannot exceed 500 characters")
                .When(x => x.DeferralReason == Domain.Enums.CarryOverDeferralReason.Other)
                .NotEmpty()
                .WithMessage("Custom deferral reason is required when deferral reason is 'Other'")
                .When(x => x.DeferralReason == Domain.Enums.CarryOverDeferralReason.Other);

            RuleFor(x => x.StockItem)
                .MaximumLength(100)
                .WithMessage("Stock item cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.StockItem));

            RuleFor(x => x.PartNumber)
                .MaximumLength(100)
                .WithMessage("Part number cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.PartNumber));

            RuleFor(x => x.StockId)
                .MaximumLength(50)
                .WithMessage("Stock ID cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.StockId));

            RuleFor(x => x.DeferredTaskOrigin)
                .IsInEnum()
                .WithMessage("Invalid deferred task origin");

            RuleFor(x => x.Timing)
                .IsInEnum()
                .WithMessage("Invalid timing");

            RuleFor(x => x.Remark)
                .MaximumLength(1000)
                .WithMessage("Remark cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Remark));

            RuleFor(x => x.DelayComment)
                .MaximumLength(500)
                .WithMessage("Delay comment cannot exceed 500 characters")
                .When(x => x.AircraftDelayImpact && !string.IsNullOrEmpty(x.DelayComment))
                .NotEmpty()
                .WithMessage("Delay comment is required when aircraft delay impact is true")
                .When(x => x.AircraftDelayImpact);

            RuleFor(x => x.DeferredUntilDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Deferred until date must be in the future")
                .When(x => x.DeferredUntilDate.HasValue);
        }
    }
}
