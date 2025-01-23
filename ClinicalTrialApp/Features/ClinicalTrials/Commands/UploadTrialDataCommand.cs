using ClinicalTrialApp.Common.Results;
using ClinicalTrialApp.Services;
using MediatR;

namespace ClinicalTrialApp.Features.ClinicalTrials.Commands;

public record UploadTrialDataCommand(IFormFile File) : IRequest<Result>;

public class UploadTrialDataCommandHandler : IRequestHandler<UploadTrialDataCommand, Result>
{
    private readonly IClinicalTrialService _trialService;

    public UploadTrialDataCommandHandler(IClinicalTrialService trialService)
    {
        _trialService = trialService;
    }

    public async Task<Result> Handle(UploadTrialDataCommand request, CancellationToken cancellationToken)
    {
        return await _trialService.ProcessTrialDataAsync(request.File.OpenReadStream(), request.File.Length, cancellationToken);
    }
}