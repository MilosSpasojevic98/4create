namespace ClinicalTrialApp.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClinicalTrialDbContext _dbContext;
    private IClinicalTrialRepository? _clinicalTrials;

    public UnitOfWork(ClinicalTrialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IClinicalTrialRepository ClinicalTrials => 
        _clinicalTrials ??= new ClinicalTrialRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 