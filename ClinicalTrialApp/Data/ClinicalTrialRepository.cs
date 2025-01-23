using ClinicalTrialApp.Models;

namespace ClinicalTrialApp.Data;

public class ClinicalTrialRepository : IClinicalTrialRepository
{
    private readonly ClinicalTrialDbContext _dbContext;

    public ClinicalTrialRepository(ClinicalTrialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClinicalTrialMetadata> AddAsync(ClinicalTrialMetadata trial)
    {
        var entry = await _dbContext.ClinicalTrialData.AddAsync(trial);
        return entry.Entity;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
} 