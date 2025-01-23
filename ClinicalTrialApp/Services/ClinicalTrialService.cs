using ClinicalTrialApp.Common.Results;
using ClinicalTrialApp.Common.Schemas;
using ClinicalTrialApp.Data;
using ClinicalTrialApp.Features.ClinicalTrials.Events;
using ClinicalTrialApp.Models;
using MediatR;
using System.Text.Json;

namespace ClinicalTrialApp.Services;

public interface IClinicalTrialService
{
    Task<Result> ProcessTrialDataAsync(Stream fileStream, long fileSize, CancellationToken cancellationToken = default);
}

public class ClinicalTrialService : IClinicalTrialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJsonSchemaValidator _schemaValidator;
    private readonly IPublisher _publisher;

    public ClinicalTrialService(
        IUnitOfWork unitOfWork,
        IJsonSchemaValidator schemaValidator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _schemaValidator = schemaValidator;
        _publisher = publisher;
    }

    public async Task<Result> ProcessTrialDataAsync(Stream fileStream, long fileSize, CancellationToken cancellationToken = default)
    {
        try
        {
            using var streamReader = new StreamReader(fileStream);
            var jsonContent = await streamReader.ReadToEndAsync();

            var (isValid, errors) = await _schemaValidator.ValidateAsync(jsonContent, SchemaType.ClinicalTrial);
            if (!isValid)
                return Result.Failure($"Invalid JSON format: {string.Join(", ", errors)}");

            var trialData = JsonSerializer.Deserialize<ClinicalTrialMetadata>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (trialData == null)
                return Result.Failure("Failed to deserialize trial data");

            ApplyBusinessRules(trialData);

            trialData.Id = Guid.NewGuid();
            trialData.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ClinicalTrials.AddAsync(trialData);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new TrialCreatedEvent(trialData), cancellationToken);

            return Result.Success();
        }
        catch (JsonException ex)
        {
            return Result.Failure($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Processing error: {ex.Message}");
        }
    }

    private void ApplyBusinessRules(ClinicalTrialMetadata trial)
    {
        trial.StartDate = DateTime.SpecifyKind(trial.StartDate, DateTimeKind.Utc);
        if (trial.EndDate.HasValue)
        {
            trial.EndDate = DateTime.SpecifyKind(trial.EndDate.Value, DateTimeKind.Utc);
        }

        if (trial.EndDate.HasValue)
        {
            trial.DurationInDays = (trial.EndDate.Value - trial.StartDate).Days;
        }

        // Set default end date for ongoing trials
        if (!trial.EndDate.HasValue && trial.Status == TrialStatus.Ongoing)
        {
            trial.EndDate = trial.StartDate.AddMonths(1);
            trial.DurationInDays = (trial.EndDate.Value - trial.StartDate).Days;
            trial.Status = TrialStatus.Ongoing;
        }
    }
}