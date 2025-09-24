using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using CardDeck.Api.Services;
using Microsoft.EntityFrameworkCore;
using CardDeck.Api.Models;
using Microsoft.Extensions.Configuration;

namespace CardDeck.Test;

public class StatusEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetStatus_WhenServicesAreMixed_ReturnsOkAndCorrectStatusBody()
    {
        // ARRANGE

        // mock IStatusService
        var mockStatusService = new Mock<IStatusService>();
        var expectedStatus = new StatusResult(new Dictionary<string, bool>
        {
            { "Database", true },
            { "ExternalApi", false }
        });
        mockStatusService
            .Setup(s => s.CheckConnectionAsync())
            .ReturnsAsync(expectedStatus);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // mock IStatusService
                var statusServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStatusService));
                if (statusServiceDescriptor != null)
                    services.Remove(statusServiceDescriptor);

                services.AddSingleton(mockStatusService.Object);

                // mock DbContext with in-memory database
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CardDeckContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                // ensure that no actual database connection is attempted
                services.AddDbContext<CardDeckContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });
            });
        }).CreateClient();

        // ACT
        var response = await client.GetAsync("/status");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // The API returns a StatusResult object with a `Status` property, e.g. { "status": { "Database": true, ... } }
        var actualResult = JsonSerializer.Deserialize<StatusResult>(responseBody, options);
        var actualStatus = actualResult?.Status;

        actualStatus.Should().NotBeNull();
        actualStatus.Should().ContainKey("Database").WhoseValue.Should().BeTrue();
        actualStatus.Should().ContainKey("ExternalApi").WhoseValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatus_ResponseJson_ContainsStatusProperty()
    {
        // ARRANGE
        var mockStatusService = new Mock<IStatusService>();
        var expectedStatus = new StatusResult(new Dictionary<string, bool>
        {
            { "Database", true },
            { "ExternalApi", false }
        });
        mockStatusService
            .Setup(s => s.CheckConnectionAsync())
            .ReturnsAsync(expectedStatus);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var statusServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStatusService));
                if (statusServiceDescriptor != null)
                    services.Remove(statusServiceDescriptor);

                services.AddSingleton(mockStatusService.Object);

                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CardDeckContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                services.AddDbContext<CardDeckContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDb2");
                });
            });
        }).CreateClient();

        // ACT
        var response = await client.GetAsync("/status");
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        // ASSERT - examine raw JSON shape
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var statusElement).Should().BeTrue("response should include a root 'status' property");
        statusElement.ValueKind.Should().Be(JsonValueKind.Object);

        // confirm keys exist and values match mocked service
        statusElement.GetProperty("Database").GetBoolean().Should().BeTrue();
        statusElement.GetProperty("ExternalApi").GetBoolean().Should().BeFalse();
    }
}
