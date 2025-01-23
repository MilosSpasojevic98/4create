using ClinicalTrialApp.Data;
using ClinicalTrialApp.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicalTrialApp.Features.ClinicalTrials.Queries;

public record GetTrialByIdQuery(Guid Id) : IRequest<ClinicalTrialMetadata?>;

public class GetTrialByIdQueryHandler : IRequestHandler<GetTrialByIdQuery, ClinicalTrialMetadata?>
{
    private readonly ClinicalTrialDbContext _dbContext;

    public GetTrialByIdQueryHandler(ClinicalTrialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClinicalTrialMetadata?> Handle(GetTrialByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ClinicalTrialData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
    }
} 