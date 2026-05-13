using FluentValidation;
using AviationMaintenance.Application.DTOs.CarryOverReport;

namespace AviationMaintenance.Application.Validators.CarryOverReport
{
    /// <summary>
    /// Validator for UpdateCarryOverTaskRequest
    /// </summary>
    public class UpdateCarryOverTaskRequestValidator : AbstractValidator<UpdateCarryOverTaskRequest>
    {
        public UpdateCarryOverTaskRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Task ID is required");

            RuleFor(x => x.TaskName)
                .MaximumLength(500)
                .WithMessage("Task name cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.TaskName));

            RuleFor(x => x.TaskBarcode)
                .MaximumLength(100)
                .WithMessage("Task barcode cannot exceed 100 characters")
                .Matches(@"^[A-Z0-9\-\/]+$")
                .WithMessage("Task barcode can only contain uppercase letters, numbers, hyphens, and forward slashes")
                .When(x => !string.IsNullOrEmpty(x.TaskBarcode));

            RuleFor(x => x.CustomDeferralReason)
                .MaximumLength(500)
                .WithMessage("Custom deferral reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.CustomDeferralReason))
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

            RuleFor(x => x.Remark)
                .MaximumLength(1000)
                .WithMessage("Remark cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Remark));

            RuleFor(x => x.DelayComment)
                .MaximumLength(500)
                .WithMessage("Delay comment cannot exceed 500 characters")
                .When(x => x.AircraftDelayImpact == true && !string.IsNullOrEmpty(x.DelayComment))
                .NotEmpty()
                .WithMessage("Delay comment is required when aircraft delay impact is true")
                .When(x => x.AircraftDelayImpact == true);

            RuleFor(x => x.DeferredUntilDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Deferred until date must be in the future")
                .When(x => x.DeferredUntilDate.HasValue);
        }
    }
}
