using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Custom WebApplicationFactory that replaces the PostgreSQL database with
/// an isolated in-memory database for each test class instance.
/// </summary>
public class SalesApiFactory : WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>, IAsyncLifetime
{
    private readonly string _dbName = "FuncTests_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove Npgsql DbContext registrations
            services.RemoveAll(typeof(DbContextOptions<DefaultContext>));
            services.RemoveAll(typeof(DefaultContext));

            // Substitute with isolated in-memory database
            services.AddDbContext<DefaultContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public async Task InitializeAsync()
    {
        // Ensure EF creates the schema for the in-memory database
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
