using ClinicalTrialApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalTrialApp.Data;

public class ClinicalTrialDbContext : DbContext
{
    public ClinicalTrialDbContext(DbContextOptions<ClinicalTrialDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClinicalTrialMetadata> ClinicalTrialData { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClinicalTrialMetadata>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ClinicalTrialMetadata>()
            .HasIndex(e => e.TrialId)
            .IsUnique();
    }
}