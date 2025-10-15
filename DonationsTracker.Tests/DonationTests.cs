
/// DONATION TEST CASE FOR CRUD + PAGINATION + SANITIZATION + 401 
using System.Net;
using System.Net.Http.Json;
using DonationsTracker.Api.Models;
using DonationsTracker.DB;
using Microsoft.AspNetCore.Mvc.Testing;

[Collection("Api collection")]
public class DonationTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _db;

    public DonationTests(DatabaseFixture db)
    {
        _db = db;
        _client = db.Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Add these headers to bypass bot protection
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (UnitTestClient)");
        _client.DefaultRequestHeaders.Add("Referer", "https://localhost/test");
    }

    private async Task<string> GetTokenAsync()
    {
        await _db.ResetAsync();

        var reg = new
        {
            firstName = "J",
            lastName = "D",
            email = "d1@test.com",
            password = "Passw0rd!",
            avatar = "a"
        };
        await _client.PostAsJsonAsync("/api/auth/register", reg);

        var login = new
        {
            email = "d1@test.com",
            password = "Passw0rd!"
        };
        var res = await _client.PostAsJsonAsync("/api/auth/login", login);
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<DonationsTracker.Core.Models.AuthResponse>>();
        return body!.Data!.Token;
    }

    [Fact]
    public async Task Unauthorized_When_Missing_Token()
    {
        var resp = await _client.GetAsync("/api/donation?page=1&limit=2");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Get_Delete_Donation_Works_With_Pagination()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        // Create
        var create = new Donation
        {
            Description = "First <script>alert(1)</script>",
            Amount = 25.50
        };
        var post = await _client.PostAsJsonAsync("/api/donation", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get page 1
        var page1 = await _client.GetAsync("/api/donation?page=1&limit=1");
        page1.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await page1.Content.ReadFromJsonAsync<ApiResponse<List<Donation>>>();
        list!.Meta.Pagination!.Total.Should().BeGreaterOrEqualTo(1);
        list.Data![0].Description.Should().NotContain("<script>");

        // Delete
        var id = list.Data![0].Id;
        var del = await _client.DeleteAsync($"/api/donation/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
