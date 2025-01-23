using ClinicalTrialApp.Models;

namespace ClinicalTrialApp.Data;

public interface IClinicalTrialRepository
{
    Task<ClinicalTrialMetadata> AddAsync(ClinicalTrialMetadata trial);
    Task SaveChangesAsync();
} 