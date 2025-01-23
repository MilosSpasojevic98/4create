using ClinicalTrialApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Application.Integration.Tests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private IServiceScope _scope;
    private ClinicalTrialDbContext _context;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("clinicalTrial")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration if exists
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ClinicalTrialDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add DbContext using the test container
            services.AddDbContext<ClinicalTrialDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString())
                       .UseSnakeCaseNamingConvention();
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Create a scope to obtain a reference to the database context
        _scope = Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ClinicalTrialDbContext>();

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        // Clean up the database
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        if (_scope != null)
            _scope.Dispose();

        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}