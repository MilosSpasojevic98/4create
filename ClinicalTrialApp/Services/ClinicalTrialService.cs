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
    Task<Result<Guid>> ProcessTrialDataAsync(string? jsonContent, CancellationToken cancellationToken = default);
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

    public async Task<Result<Guid>> ProcessTrialDataAsync(string? jsonContent, CancellationToken cancellationToken = default)
    {
        try
        {
            var (isValid, errors) = await _schemaValidator.ValidateAsync(jsonContent, SchemaType.ClinicalTrial);
            if (!isValid)
                return Result<Guid>.Failure($"Invalid JSON format: {string.Join(", ", errors)}");

            var trialData = JsonSerializer.Deserialize<ClinicalTrialMetadata>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (trialData == null)
                return Result<Guid>.Failure("Failed to deserialize trial data");

            // Check if trial with same trialId already exists
            var existingTrial = await _unitOfWork.ClinicalTrials.FirstOrDefaultAsync(t => t.TrialId == trialData.TrialId);

            if (existingTrial != null)
                return Result<Guid>.Failure($"Trial with ID '{trialData.TrialId}' has already been uploaded");

            ApplyBusinessRules(trialData);

            trialData.Id = Guid.NewGuid();
            trialData.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ClinicalTrials.AddAsync(trialData);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new TrialCreatedEvent(trialData), cancellationToken);

            return Result<Guid>.Success(trialData.Id);
        }
        catch (JsonException ex)
        {
            return Result<Guid>.Failure($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Processing error: {ex.Message}");
        }
    }

    private void ApplyBusinessRules(ClinicalTrialMetadata trial)
    {
        trial.StartDate = DateTime.SpecifyKind(trial.StartDate, DateTimeKind.Utc);
        if (trial.EndDate.HasValue)
        {
            trial.EndDate = DateTime.SpecifyKind(trial.EndDate.Value, DateTimeKind.Utc);
            trial.DurationInDays = (trial.EndDate.Value - trial.StartDate).Days;
        }

        // Set default end date for ongoing trials
        if (!trial.EndDate.HasValue && trial.Status == TrialStatus.Ongoing)
        {
            trial.EndDate = trial.StartDate.AddMonths(1);
            trial.DurationInDays = (trial.EndDate.Value - trial.StartDate).Days;
        }
    }
}