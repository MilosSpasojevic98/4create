using ClinicalTrialApp.Common.Results;
using ClinicalTrialApp.Services;
using MediatR;

namespace ClinicalTrialApp.Features.ClinicalTrials.Commands;

public record UploadTrialDataCommand(IFormFile File) : IRequest<Result<Guid>>;

public class UploadTrialDataCommandHandler : IRequestHandler<UploadTrialDataCommand, Result<Guid>>
{
    private readonly IClinicalTrialService _trialService;
    private readonly ILogger<UploadTrialDataCommandHandler> _logger;

    public UploadTrialDataCommandHandler(
        IClinicalTrialService trialService,
        ILogger<UploadTrialDataCommandHandler> logger)
    {
        _trialService = trialService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UploadTrialDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            using var streamReader = new StreamReader(request.File.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync(cancellationToken);

            return await _trialService.ProcessTrialDataAsync(jsonContent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial data upload");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}