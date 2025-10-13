using System.Net;
using System.Net.Http.Json;
using DonationsTracker.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

[Collection("Api collection")]
public class SecurityTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _db;

    public SecurityTests(DatabaseFixture db)
    {
        _db = db;
        _client = db.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task BotUserAgent_Is_Rejected()
    {
        // Arrange
        await _db.ResetAsync();
        var token = await _db.GetValidTokenAsync();

        var donation = new
        {
            description = "Possible bot payload",
            amount = 10.0
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/donation")
        {
            Content = JsonContent.Create(donation)
        };
        req.Headers.UserAgent.ParseAdd("GoogleBot/2.1");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var res = await _client.SendAsync(req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await res.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body.Should().NotBeNull();
        body!.Errors.Should().NotBeNull();
        body.Errors!.Should().HaveCount(1);
        body.Errors![0].Code.Should().Be(ErrorCode.VALIDATION_ERROR);
        body.Errors![0].Message.Should().Contain("Suspicious activity");
    }

    [Fact]
    public async Task Malformed_Json_Returns_400()
    {
        // Arrange
        await _db.ResetAsync();
        var token = await _db.GetValidTokenAsync();

        var badJson = "{ description: 'Missing quotes }";
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/donation")
        {
            Content = new StringContent(badJson, System.Text.Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var res = await _client.SendAsync(req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await res.Content.ReadAsStringAsync();
        body.Should().MatchRegex("(?i)(bad request|error|invalid json)");
    }

}
