using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DonationsTracker.DB;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Json;
using DonationsTracker.Api.Models;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly string _masterConnectionString;

    public WebApplicationFactory<Program> Factory { get; private set; }

    public DatabaseFixture()
    {
        // Load config from appsettings.Test.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection missing in appsettings.Test.json");

        _masterConnectionString = _connectionString.Replace("Database=DonationTracker_TEST", "Database=master");

        // ✅ Create factory immediately
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
                builder.UseEnvironment("Testing");
            });
    }

    public async Task InitializeAsync()
    {
        // Drop and recreate DB cleanly
        await using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        var sql = @"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DonationTracker_TEST')
            BEGIN
                ALTER DATABASE [DonationTracker_TEST] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [DonationTracker_TEST];
            END;
            CREATE DATABASE [DonationTracker_TEST];
        ";

        await using (var command = new SqlCommand(sql, connection))
            await command.ExecuteNonQueryAsync();

        await connection.CloseAsync();

        // Apply EF migrations
        var options = new DbContextOptionsBuilder<DonationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        await using (var context = new DonationDbContext(options))
            await context.Database.MigrateAsync();
    }

    public async Task ResetAsync()
    {
        var options = new DbContextOptionsBuilder<DonationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        await using var context = new DonationDbContext(options);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM Donations;");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers;");
    }

    public async Task DisposeAsync()
    {
        try
        {
            await using var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync();

            var drop = @"
                ALTER DATABASE [DonationTracker_TEST] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [DonationTracker_TEST];
            ";
            await using var command = new SqlCommand(drop, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Warning] Cleanup failed: {ex.Message}");
        }
    }

    public async Task<string> GetValidTokenAsync()
    {
        using var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Ubuntu; Linux x86_64) AppleWebKit/537.36 Chrome/118.0 Safari/537.36");

        var email = $"tokenuser_{Guid.NewGuid():N}@test.com";

        // Register a user
        var register = new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "Passw0rd!",
            avatar = "img"
        };
        await client.PostAsJsonAsync("/api/auth/register", register);

        // Login to get token
        var login = new
        {
            email,
            password = "Passw0rd!"
        };
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", login);
        var content = await loginRes.Content.ReadAsStringAsync();

        if (!loginRes.IsSuccessStatusCode)
            throw new Exception($"Login failed: {loginRes.StatusCode} | Body: {content}");

        var body = await loginRes.Content.ReadFromJsonAsync<ApiResponse<DonationsTracker.Core.Models.AuthResponse>>();
        if (body?.Data?.Token == null)
            throw new Exception("Failed to get token: Response body was null or malformed");

        return body.Data.Token;
    }
}
