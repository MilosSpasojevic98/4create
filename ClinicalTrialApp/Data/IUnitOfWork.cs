namespace ClinicalTrialApp.Data;

public interface IUnitOfWork
{
    IClinicalTrialRepository ClinicalTrials { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 