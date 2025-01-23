using ClinicalTrialApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Integration.Tests;

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly ClinicalTrialDbContext DbContext;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<ClinicalTrialDbContext>();
    }

    public virtual async Task InitializeAsync()
    {
        // Reset database state before each test
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        Scope.Dispose();
    }
}