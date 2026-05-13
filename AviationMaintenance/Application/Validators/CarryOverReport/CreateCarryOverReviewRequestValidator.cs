using FluentValidation;
using AviationMaintenance.Application.DTOs.CarryOverReport;

namespace AviationMaintenance.Application.Validators.CarryOverReport
{
    /// <summary>
    /// Validator for CreateCarryOverReviewRequest
    /// </summary>
    public class CreateCarryOverReviewRequestValidator : AbstractValidator<CreateCarryOverReviewRequest>
    {
        public CreateCarryOverReviewRequestValidator()
        {
            RuleFor(x => x.CarryOverReportId)
                .NotEmpty()
                .WithMessage("Carry-over report ID is required");

            RuleFor(x => x.ReviewerRole)
                .IsInEnum()
                .WithMessage("Invalid reviewer role");

            RuleFor(x => x.Action)
                .IsInEnum()
                .WithMessage("Invalid review action");

            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .WithMessage("Comment cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Comment))
                .NotEmpty()
                .WithMessage("Comment is required when action is 'RequiresChanges' or 'Rejected'")
                .When(x => x.Action == Domain.Enums.ReviewAction.RequiresChanges || x.Action == Domain.Enums.ReviewAction.Rejected);
        }
    }
}
