using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DonationsTracker.DB;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    public string EnvName => "Test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvName);

        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"), optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DonationDbContext>();

            // Use EnsureCreated instead of Migrate for tests
            db.Database.EnsureCreated();
        });

    }
}
