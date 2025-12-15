using AiForge.Application.DTOs.Estimation;
using AiForge.Domain.Enums;
using FluentValidation;

namespace AiForge.Application.Validators;

public class ReviseEstimationRequestValidator : AbstractValidator<ReviseEstimationRequest>
{
    public ReviseEstimationRequestValidator()
    {
        RuleFor(x => x.Complexity)
            .NotEmpty()
            .WithMessage("Complexity is required")
            .Must(BeValidComplexityLevel)
            .WithMessage("Invalid complexity level. Valid values: Low, Medium, High, VeryHigh");

        RuleFor(x => x.EstimatedEffort)
            .NotEmpty()
            .WithMessage("Estimated effort is required")
            .Must(BeValidEffortSize)
            .WithMessage("Invalid effort size. Valid values: XSmall, Small, Medium, Large, XLarge");

        RuleFor(x => x.ConfidencePercent)
            .InclusiveBetween(0, 100)
            .WithMessage("Confidence must be between 0 and 100");

        RuleFor(x => x.EstimationReasoning)
            .NotEmpty()
            .WithMessage("Estimation reasoning is required for transparency")
            .MaximumLength(2000)
            .WithMessage("Estimation reasoning cannot exceed 2000 characters");

        RuleFor(x => x.Assumptions)
            .MaximumLength(1000)
            .WithMessage("Assumptions cannot exceed 1000 characters")
            .When(x => x.Assumptions != null);

        RuleFor(x => x.RevisionReason)
            .NotEmpty()
            .WithMessage("Revision reason is required when revising an estimate")
            .MaximumLength(500)
            .WithMessage("Revision reason cannot exceed 500 characters");

        RuleFor(x => x.SessionId)
            .MaximumLength(100)
            .WithMessage("Session ID cannot exceed 100 characters")
            .When(x => x.SessionId != null);
    }

    private static bool BeValidComplexityLevel(string complexity)
    {
        return Enum.TryParse<ComplexityLevel>(complexity, ignoreCase: true, out _);
    }

    private static bool BeValidEffortSize(string effort)
    {
        return Enum.TryParse<EffortSize>(effort, ignoreCase: true, out _);
    }
}
