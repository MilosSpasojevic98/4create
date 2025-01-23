using ClinicalTrialApp.Data;
using ClinicalTrialApp.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicalTrialApp.Features.ClinicalTrials.Queries;

public record GetTrialsQuery(TrialStatus? Status = null) : IRequest<IEnumerable<ClinicalTrialMetadata>>;

public class GetTrialsQueryHandler : IRequestHandler<GetTrialsQuery, IEnumerable<ClinicalTrialMetadata>>
{
    private readonly ClinicalTrialDbContext _dbContext;

    public GetTrialsQueryHandler(ClinicalTrialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ClinicalTrialMetadata>> Handle(GetTrialsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ClinicalTrialData.AsNoTracking();

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        return await query.ToListAsync(cancellationToken);
    }
} 