using ClinicalTrialApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalTrialApp.Data;

public class ClinicalTrialRepository : Repository<ClinicalTrialMetadata>, IClinicalTrialRepository
{
    public ClinicalTrialRepository(ClinicalTrialDbContext context) : base(context)
    {
    }
}