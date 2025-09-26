using System.Net;
using System.Net.Http.Json;
using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CardDeck.Test;

public class DeckTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // --- Test Data Helpers ---
    private static readonly DateTimeOffset TestTimestamp = DateTimeOffset.UnixEpoch;
    private static readonly SuitDTO HeartsSuit = new(1, "Hearts", '♥', 16711680);
    private static readonly SuitDTO SpadesSuit = new(4, "Spades", '♠', 0);
    private static readonly CardDTO AceOfSpades = new(
        1,
        "A",
        SpadesSuit,
        [new CardEffect("+", 1), new CardEffect("+", 11)],
        TestTimestamp,
        TestTimestamp
    );
    private static readonly CardDTO KingOfHearts = new(
        2,
        "K",
        HeartsSuit,
        [new CardEffect("+", 10)],
        TestTimestamp,
        TestTimestamp
    );
    private static readonly AvailableCardDTO AvailableAceOfSpades = new(
        101,
        AceOfSpades,
        TestTimestamp
    );
    private static readonly AvailableCardDTO AvailableKingOfHearts = new(
        102,
        KingOfHearts,
        TestTimestamp
    );

    #region GET /deck Tests
    [Fact]
    public async Task GetAllAvailableCards_WhenCardsExist_ReturnsOkAndListOfAvailableCards()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        var expectedAvailableCards = new List<AvailableCardDTO>
        {
            AvailableAceOfSpades,
            AvailableKingOfHearts,
        };
        mockAvailableCardService
            .Setup(s => s.GetAllAvailableCardsAsync())
            .ReturnsAsync(expectedAvailableCards);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.GetAsync("/deck");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualAvailableCards = await response.Content.ReadFromJsonAsync<List<AvailableCardDTO>>(
            _jsonOptions
        );
        actualAvailableCards.Should().BeEquivalentTo(expectedAvailableCards);
    }

    [Fact]
    public async Task GetAllAvailableCards_WhenNoCardsExist_ReturnsOkAndEmptyList()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        mockAvailableCardService.Setup(s => s.GetAllAvailableCardsAsync()).ReturnsAsync([]);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.GetAsync("/deck");

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualAvailableCards = await response.Content.ReadFromJsonAsync<List<AvailableCardDTO>>(
            _jsonOptions
        );
        actualAvailableCards.Should().BeEmpty();
    }
    #endregion

    #region POST /deck Tests
    [Fact]
    public async Task CreateAvailableCard_WithValidData_ReturnsOkAndAvailableCard()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        var createDTO = new CreateAvailableCardDTO(AceOfSpades.Id);
        mockAvailableCardService
            .Setup(s => s.InsertAvailableCardAsync(It.IsAny<CreateAvailableCardDTO>()))
            .ReturnsAsync(AvailableAceOfSpades);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsJsonAsync("/deck", createDTO);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Endpoint returns 200 OK
        var actualAvailableCard = await response.Content.ReadFromJsonAsync<AvailableCardDTO>(
            _jsonOptions
        );
        actualAvailableCard.Should().BeEquivalentTo(AvailableAceOfSpades);
    }

    [Fact]
    public async Task CreateAvailableCard_WithInvalidCardId_ReturnsBadRequest()
    {
        // ARRANGE
        var client = CreateTestClient(); // uses real client to test validation
        var invalidDTO = new CreateAvailableCardDTO(0);

        // ACT
        var response = await client.PostAsJsonAsync("/deck", invalidDTO);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
            _jsonOptions
        );
        problem!.Errors.Should().ContainKey("CardId");
        problem!.Errors["CardId"].Should().Contain("Valid CardId is required.");
    }

    [Fact]
    public async Task CreateAvailableCard_WhenReferencedCardDoesNotExist_ReturnsBadRequest()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        var createDTO = new CreateAvailableCardDTO(999); // Non-existent card ID
        mockAvailableCardService
            .Setup(s => s.InsertAvailableCardAsync(It.IsAny<CreateAvailableCardDTO>()))
            .ThrowsAsync(new BadRequestException("Card with ID 999 does not exist."));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsJsonAsync("/deck", createDTO);

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // deserialize to ProblemDetails
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Detail.Should().Contain("Card with ID 999 does not exist.");
    }
    #endregion

    #region POST /deck/draw/{numberOfCards:int} Tests
    [Fact]
    public async Task DrawCards_WhenCardsExist_ReturnsOkAndListOfDrawnCards()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        var expectedDrawnCards = new List<CardDTO> { AceOfSpades, KingOfHearts };
        mockAvailableCardService
            .Setup(s => s.DrawRandomCardsAsync(2))
            .ReturnsAsync(expectedDrawnCards);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsync(
            "/deck/draw/2",
            new StringContent("", System.Text.Encoding.UTF8, "application/json")
        );

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualDrawnCards = await response.Content.ReadFromJsonAsync<List<CardDTO>>(
            _jsonOptions
        );
        actualDrawnCards.Should().BeEquivalentTo(expectedDrawnCards);
    }

    [Fact]
    public async Task DrawCards_WhenNoCardsExist_ReturnsNotFound()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        mockAvailableCardService
            .Setup(s => s.DrawRandomCardsAsync(1))
            .ThrowsAsync(new NotFoundException("No cards available in the deck to draw."));

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsync(
            "/deck/draw/1",
            new StringContent("", System.Text.Encoding.UTF8, "application/json")
        );

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // deserialize to ProblemDetails
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails!.Status.Should().Be((int)HttpStatusCode.NotFound);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Detail.Should().Contain("No cards available in the deck to draw.");
    }

    [Fact]
    public async Task DrawCards_WhenDrawingMoreCardsThanAvailable_ReturnsAvailableCards()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        // Assume only one card is actually available and drawn
        var expectedDrawnCards = new List<CardDTO> { AceOfSpades };
        mockAvailableCardService
            .Setup(s => s.DrawRandomCardsAsync(5)) // requests 5, should return 1
            .ReturnsAsync(expectedDrawnCards);

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsync(
            "/deck/draw/5",
            new StringContent("", System.Text.Encoding.UTF8, "application/json")
        );

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualDrawnCards = await response.Content.ReadFromJsonAsync<List<CardDTO>>(
            _jsonOptions
        );
        actualDrawnCards.Should().BeEquivalentTo(expectedDrawnCards);
        actualDrawnCards.Should().HaveCount(1); // only one card was "available" in the mock
    }

    [Fact]
    public async Task DrawCards_WhenConcurrencyConflictOccurs_ReturnsConflict()
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        mockAvailableCardService
            .Setup(s => s.DrawRandomCardsAsync(1))
            .ThrowsAsync(
                new ConflictException(
                    "Failed to draw any cards due to concurrent modifications. Please try again."
                )
            );

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsync(
            "/deck/draw/1",
            new StringContent("", System.Text.Encoding.UTF8, "application/json")
        );

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        // deserialize to ProblemDetails
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails!.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Title.Should().Be("Conflict");
        problemDetails.Detail.Should().Contain("concurrent modifications");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DrawCards_WithZeroOrNegativeCount_ReturnsOkAndEmptyList(int count)
    {
        // ARRANGE
        var mockAvailableCardService = new Mock<IAvailableCardService>();
        mockAvailableCardService.Setup(s => s.DrawRandomCardsAsync(count)).ReturnsAsync([]); // Service returns empty list for <= 0 count

        var client = CreateTestClient(services =>
        {
            services.RemoveAll<IAvailableCardService>();
            services.AddSingleton(mockAvailableCardService.Object);
        });

        // ACT
        var response = await client.PostAsync(
            $"/deck/draw/{count}",
            new StringContent("", System.Text.Encoding.UTF8, "application/json")
        );

        // ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualDrawnCards = await response.Content.ReadFromJsonAsync<List<CardDTO>>(
            _jsonOptions
        );
        actualDrawnCards.Should().BeEmpty();
    }
    #endregion
}
