using ClinicalTrialApp.Features.ClinicalTrials.Commands;
using FluentValidation;

namespace ClinicalTrialApp.Features.ClinicalTrials.Validators;

public class UploadTrialDataCommandValidator : AbstractValidator<UploadTrialDataCommand>
{
    public UploadTrialDataCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(1024 * 1024)
            .WithMessage("File size must not exceed 1MB");

        RuleFor(x => x.File.FileName)
            .Must(x => x.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only JSON files are accepted");
    }
} 