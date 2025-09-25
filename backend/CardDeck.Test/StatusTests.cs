using System.Net;
using System.Text.Json;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CardDeck.Test;

public class StatusEndpointTests(WebApplicationFactory<Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetStatus_WhenServicesAreMixed_ReturnsOkAndCorrectStatusBody()
    {
        // ARRANGE

        // mock IStatusService
        var mockStatusService = new Mock<IStatusService>();
        var expectedStatus = new StatusDTO(
            new Dictionary<string, bool> { { "Database", true }, { "ExternalApi", false } }
        );
        mockStatusService.Setup(s => s.CheckConnectionAsync()).ReturnsAsync(expectedStatus);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IStatusService>();
            services.AddSingleton(mockStatusService.Object);
        });

        // ACT
        var response = await client.GetAsync("/status");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // The API returns a StatusResult object with a `Status` property, e.g. { "status": { "Database": true, ... } }
        var actualResult = JsonSerializer.Deserialize<StatusDTO>(responseBody, options);
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
        var expectedStatus = new StatusDTO(
            new Dictionary<string, bool> { { "Database", true }, { "ExternalApi", false } }
        );
        mockStatusService.Setup(s => s.CheckConnectionAsync()).ReturnsAsync(expectedStatus);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IStatusService>();
            services.AddSingleton(mockStatusService.Object);
        });

        // ACT
        var response = await client.GetAsync("/status");
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        // ASSERT - examine raw JSON shape
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var statusElement)
            .Should()
            .BeTrue("response should include a root 'status' property");
        statusElement.ValueKind.Should().Be(JsonValueKind.Object);

        // confirm keys exist and values match mocked service
        statusElement.GetProperty("Database").GetBoolean().Should().BeTrue();
        statusElement.GetProperty("ExternalApi").GetBoolean().Should().BeFalse();
    }
}
