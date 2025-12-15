using AiForge.Application.DTOs.Estimation;
using AiForge.Domain.Enums;
using FluentValidation;

namespace AiForge.Application.Validators;

public class RecordActualEffortRequestValidator : AbstractValidator<RecordActualEffortRequest>
{
    public RecordActualEffortRequestValidator()
    {
        RuleFor(x => x.ActualEffort)
            .NotEmpty()
            .WithMessage("Actual effort is required")
            .Must(BeValidEffortSize)
            .WithMessage("Invalid effort size. Valid values: XSmall, Small, Medium, Large, XLarge");

        RuleFor(x => x.VarianceNotes)
            .MaximumLength(1000)
            .WithMessage("Variance notes cannot exceed 1000 characters")
            .When(x => x.VarianceNotes != null);
    }

    private static bool BeValidEffortSize(string effort)
    {
        return Enum.TryParse<EffortSize>(effort, ignoreCase: true, out _);
    }
}
