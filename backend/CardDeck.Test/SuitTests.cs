using System.Net;
using System.Net.Http.Json;
using CardDeck.Api.Exceptions;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CardDeck.Test;

public class SuitTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllSuits_WhenSuitsExist_ReturnsOkAndListOfSuits()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var expectedSuits = new List<SuitDTO>
        {
            new(1, "Hearts", '♥', 1),
            new(2, "Diamonds", '♦', 2),
        };
        mockSuitService.Setup(s => s.GetAllSuitsAsync()).ReturnsAsync(expectedSuits);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.GetAsync("/suits");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualSuits = await response.Content.ReadFromJsonAsync<List<SuitDTO>>(_jsonOptions);
        actualSuits.Should().BeEquivalentTo(expectedSuits);
    }

    [Fact]
    public async Task GetSuitById_WhenSuitExists_ReturnsOkAndSuit()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var expectedSuit = new SuitDTO(1, "Hearts", '♥', 1);
        mockSuitService.Setup(s => s.GetSuitByIdAsync(1)).ReturnsAsync(expectedSuit);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.GetAsync("/suits/1");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualSuit = await response.Content.ReadFromJsonAsync<SuitDTO>(_jsonOptions);
        actualSuit.Should().BeEquivalentTo(expectedSuit);
    }

    [Fact]
    public async Task GetSuitById_WhenSuitDoesNotExist_ReturnsNotFound()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var expectedErrorMessage = "Suit with ID 999 was not found.";
        mockSuitService
            .Setup(s => s.GetSuitByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException(expectedErrorMessage));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.GetAsync("/suits/999");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateSuit_WithValidData_ReturnsCreatedAndSuit()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var newSuitDto = new CreateSuitDTO("Clubs", '♣', 3);
        var expectedSuit = new SuitDTO(3, "Clubs", '♣', 3);
        mockSuitService
            .Setup(s => s.CreateSuitAsync(It.IsAny<CreateSuitDTO>()))
            .ReturnsAsync(expectedSuit);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PostAsJsonAsync("/suits", newSuitDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().Be("/suits/3");
        var actualSuit = await response.Content.ReadFromJsonAsync<SuitDTO>(_jsonOptions);
        actualSuit.Should().BeEquivalentTo(expectedSuit);
    }

    [Fact]
    public async Task UpdateSuit_WhenSuitExists_ReturnsNoContent()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var updateDto = new UpdateSuitDTO("Updated Spades", '♠', 4);
        mockSuitService
            .Setup(s => s.UpdateSuitAsync(1, It.IsAny<UpdateSuitDTO>()))
            .Returns(Task.CompletedTask);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PutAsJsonAsync("/suits/1", updateDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSuit_WhenSuitDoesNotExist_ReturnsNotFound()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var updateDto = new UpdateSuitDTO("Does not exist", 'X', 0);
        var expectedErrorMessage = "Suit with ID 999 not found for update.";
        mockSuitService
            .Setup(s => s.UpdateSuitAsync(999, It.IsAny<UpdateSuitDTO>()))
            .ThrowsAsync(new NotFoundException(expectedErrorMessage));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PutAsJsonAsync("/suits/999", updateDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSuit_WhenConcurrencyConflictOccurs_ReturnsConflict()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var updateDto = new UpdateSuitDTO("Updated Spades", '♠', 4);
        var expectedErrorMessage =
            "The suit you are trying to update has been modified by another user. Please refresh and try again.";

        mockSuitService
            .Setup(s => s.UpdateSuitAsync(1, It.IsAny<UpdateSuitDTO>()))
            .ThrowsAsync(new ConflictException(expectedErrorMessage));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PutAsJsonAsync("/suits/1", updateDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await response.Content.ReadFromJsonAsync<ApiExceptionResponse>(_jsonOptions);
        error.Should().NotBeNull();
        error!.Status.Should().Be((int)HttpStatusCode.Conflict);
        error.Title.Should().Be("Conflict");
        error.Detail.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task PartialUpdateSuit_WhenSuitExists_ReturnsNoContent()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var partialDto = new PartialUpdateSuitDTO { Name = "New Patched Name" };

        mockSuitService
            .Setup(s => s.PartialUpdateSuitAsync(1, It.IsAny<PartialUpdateSuitDTO>()))
            .Returns(Task.CompletedTask);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PatchAsJsonAsync("/suits/1", partialDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PartialUpdateSuit_WhenSuitDoesNotExist_ReturnsNotFound()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var partialDto = new PartialUpdateSuitDTO { Name = "Does not matter" };
        var expectedErrorMessage = "Suit with ID 999 not found for patch.";

        mockSuitService
            .Setup(s => s.PartialUpdateSuitAsync(999, It.IsAny<PartialUpdateSuitDTO>()))
            .ThrowsAsync(new NotFoundException(expectedErrorMessage));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.PatchAsJsonAsync("/suits/999", partialDto);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ApiExceptionResponse>(_jsonOptions);
        error.Should().NotBeNull();
        error!.Status.Should().Be((int)HttpStatusCode.NotFound);
        error.Title.Should().Be("Not Found");
        error.Detail.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task DeleteSuit_WhenSuitExists_ReturnsNoContent()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        mockSuitService.Setup(s => s.DeleteSuitAsync(1)).Returns(Task.CompletedTask);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.DeleteAsync("/suits/1");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSuit_WhenSuitDoesNotExist_ReturnsNotFound()
    {
        // ARRANGE
        var mockSuitService = new Mock<ISuitService>();
        var expectedErrorMessage = "Suit with ID 999 not found for deletion.";
        mockSuitService
            .Setup(s => s.DeleteSuitAsync(999))
            .ThrowsAsync(new NotFoundException(expectedErrorMessage));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<ISuitService>();
            services.AddSingleton(mockSuitService.Object);
        });

        // ACT
        var response = await client.DeleteAsync("/suits/999");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
