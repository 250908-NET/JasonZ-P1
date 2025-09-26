using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;
using CardDeck.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardDeck.Test.Service;

public class BlackjackServiceTests
{
    private readonly Mock<IGameRepository> _mockGameRepo;
    private readonly Mock<ILogger<BlackjackService>> _mockLogger;
    private readonly BlackjackService _sut; // System Under Test

    public BlackjackServiceTests()
    {
        _mockGameRepo = new Mock<IGameRepository>();
        _mockLogger = new Mock<ILogger<BlackjackService>>();
        // i dunno what SUT means i will be honest
        _sut = new BlackjackService(_mockGameRepo.Object, _mockLogger.Object);
    }

    // --- Test Data Helper ---
    private static Card CreateCard(int id, string rank, decimal value)
    {
        return new Card
        {
            Id = id,
            Rank = rank,
            Suit = new Suit { Id = 1, Name = "Test Suit" },
            Effects = [new CardEffect("+", value)],
        };
    }

    private static Game CreateGame(Guid id, GameStatus status, decimal money, decimal bet = 0)
    {
        return new Game(id)
        {
            Status = status,
            Money = money,
            Bet = bet,
            Target = 21,
            OverdrawLimit = 1,
            OverdrawsRemaining = 1,
        };
    }

    #region POST /blackjack tests
    [Fact]
    public async Task StartNewGameAsync_WithDefaultOptions_CreatesGameWithDefaults()
    {
        // Arrange
        var dto = new StartGameDTO();
        _mockGameRepo.Setup(r => r.CreateGameAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g); // Return the game that was passed in

        // Act
        var result = await _sut.StartNewGameAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Target.Should().Be(21.0m);
        result.DrawLimit.Should().Be(5);
        result.Money.Should().Be(1000.0m);
        result.Status.Should().Be(GameStatus.RoundEnd);
        _mockGameRepo.Verify(
            r => r.CreateGameAsync(It.Is<Game>(g => g.Money == 1000.0m)),
            Times.Once
        );
    }

    [Fact]
    public async Task StartNewGameAsync_WithCustomOptions_CreatesGameWithSpecifiedValues()
    {
        // Arrange
        var dto = new StartGameDTO(Target: 31, DrawLimit: 3, InitialMoney: 500);
        _mockGameRepo.Setup(r => r.CreateGameAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        var result = await _sut.StartNewGameAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Target.Should().Be(31);
        result.DrawLimit.Should().Be(3);
        result.Money.Should().Be(500);
        _mockGameRepo.Verify(r => r.CreateGameAsync(It.Is<Game>(g => g.Target == 31)), Times.Once);
    }
    #endregion

    #region POST /blackjack/{gameId}/bet tests
    [Fact]
    public async Task StartRoundAsync_WithValidBet_StartsRoundAndDealsCards()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.RoundEnd, 1000);
        var betDto = new PlaceBetDTO(100);
        var cardToDeal = CreateCard(1, "10", 10);

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);
        _mockGameRepo.Setup(r => r.GetRandomAvailableCardAsync(gameId)).ReturnsAsync(cardToDeal);

        // simulating DB changes by mutating object
        _mockGameRepo.Setup(r => r.ClearGameCardsAsync(gameId)).Callback(game.GameCards.Clear);

        _mockGameRepo
            .Setup(r => r.AddCardToGameAsync(It.IsAny<GameCard>()))
            .Callback<GameCard>(gc =>
            {
                // We need to attach the card details to the GameCard
                // as the service logic doesn't do this part.
                gc.Card = cardToDeal;
                game.GameCards.Add(gc);
            });

        // Act
        var result = await _sut.StartRoundAsync(gameId, betDto);

        // Assert
        result.Status.Should().Be(GameStatus.DealingToPlayer);
        result.Money.Should().Be(900);
        result.Bet.Should().Be(100);
        result.Round.Should().Be(1);
        result.PlayerHand.Should().HaveCount(2);
        result.DealerHand.Should().HaveCount(1);
        _mockGameRepo.Verify(r => r.ClearGameCardsAsync(gameId), Times.Once);
        _mockGameRepo.Verify(r => r.UpdateGameAsync(It.IsAny<Game>()), Times.Once);
        _mockGameRepo.Verify(r => r.AddCardToGameAsync(It.IsAny<GameCard>()), Times.Exactly(3));
    }

    [Fact]
    public async Task StartRoundAsync_WhenNotRoundEnd_ThrowsBadRequestException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 1000);
        var betDto = new PlaceBetDTO(100);
        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.StartRoundAsync(gameId, betDto));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public async Task StartRoundAsync_WithInvalidBetAmount_ThrowsBadRequestException(
        decimal betAmount
    )
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.RoundEnd, 1000);
        var betDto = new PlaceBetDTO(betAmount);
        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.StartRoundAsync(gameId, betDto));
    }
    #endregion

    #region POST /blackjack/{gameId}/hit tests
    [Fact]
    public async Task PlayerHitAsync_WhenPlayerTurn_DealsOneCard()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "5", 5), OwnerType = OwnerType.Player }
        );

        var newCard = CreateCard(2, "6", 6);
        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);
        _mockGameRepo.Setup(r => r.GetRandomAvailableCardAsync(gameId)).ReturnsAsync(newCard);

        // When a card is added, update the game object's card list to simulate the DB update.
        _mockGameRepo
            .Setup(r => r.AddCardToGameAsync(It.IsAny<GameCard>()))
            .Callback<GameCard>(gc =>
            {
                gc.Card = newCard;
                game.GameCards.Add(gc);
            });

        // Act
        var result = await _sut.PlayerHitAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.DealingToPlayer);
        result.PlayerHand.Should().HaveCount(2); // More robust assertion
        _mockGameRepo.Verify(
            r => r.AddCardToGameAsync(It.Is<GameCard>(gc => gc.OwnerType == OwnerType.Player)),
            Times.Once
        );
    }

    [Fact]
    public async Task PlayerHitAsync_WhenPlayerBusts_EndsRound()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        game.OverdrawsRemaining = 0; // No overdraws left
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "10", 10), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(2, "7", 7), OwnerType = OwnerType.Player }
        );

        // The hit card will cause a bust
        var bustCard = CreateCard(3, "5", 5);
        _mockGameRepo.Setup(r => r.GetRandomAvailableCardAsync(gameId)).ReturnsAsync(bustCard);

        // When a card is added, update the game object's card list.
        _mockGameRepo
            .Setup(r => r.AddCardToGameAsync(It.IsAny<GameCard>()))
            .Callback<GameCard>(gc =>
            {
                gc.Card = bustCard;
                game.GameCards.Add(gc);
            });

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _sut.PlayerHitAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.RoundEnd);
        _mockGameRepo.Verify(
            r => r.UpdateGameAsync(It.Is<Game>(g => g.Status == GameStatus.RoundEnd)),
            Times.Once
        );
    }

    [Fact]
    public async Task PlayerHitAsync_WhenNotPlayerTurn_ThrowsBadRequestException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.RoundEnd, 1000);
        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.PlayerHitAsync(gameId));
    }
    #endregion

    #region POST /blackjack/{gameId}/stand tests
    [Fact]
    public async Task PlayerStandAsync_PlayerWins_AwardsWinnings()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        // Player has 20
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "10", 10), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(2, "K", 10), OwnerType = OwnerType.Player }
        );
        // Dealer has 17
        game.GameCards.Add(
            new GameCard { Card = CreateCard(3, "7", 7), OwnerType = OwnerType.Dealer }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(4, "Q", 10), OwnerType = OwnerType.Dealer }
        );

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _sut.PlayerStandAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.RoundEnd);
        result.Money.Should().Be(1100); // 900 start + 100 bet back + 100 winnings
        _mockGameRepo.Verify(
            r => r.UpdateGameAsync(It.Is<Game>(g => g.Money == 1100)),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task PlayerStandAsync_DealerBusts_PlayerWins()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        // Player has 18
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "8", 8), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(2, "K", 10), OwnerType = OwnerType.Player }
        );
        // Dealer has 16
        game.GameCards.Add(
            new GameCard { Card = CreateCard(3, "6", 6), OwnerType = OwnerType.Dealer }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(4, "Q", 10), OwnerType = OwnerType.Dealer }
        );

        // Dealer hits and gets a 10, busting them
        var bustCard = CreateCard(5, "10", 10);
        _mockGameRepo.Setup(r => r.GetRandomAvailableCardAsync(gameId)).ReturnsAsync(bustCard);

        // When a card is added, update the game object's card list.
        _mockGameRepo
            .Setup(r => r.AddCardToGameAsync(It.IsAny<GameCard>()))
            .Callback<GameCard>(gc =>
            {
                gc.Card = bustCard;
                game.GameCards.Add(gc);
            });

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _sut.PlayerStandAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.RoundEnd);
        result.Money.Should().Be(1100);
    }

    [Fact]
    public async Task PlayerStandAsync_Push_ReturnsBet()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        // Both have 20
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "10", 10), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(2, "K", 10), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(3, "J", 10), OwnerType = OwnerType.Dealer }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(4, "Q", 10), OwnerType = OwnerType.Dealer }
        );

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _sut.PlayerStandAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.RoundEnd);
        result.Money.Should().Be(1000); // 1000 start - 100 bet + 100 bet back
    }

    [Fact]
    public async Task PlayerStandAsync_PlayerLoses_LosesBet()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = CreateGame(gameId, GameStatus.DealingToPlayer, 900, 100);
        // Player has 19, Dealer has 20
        game.GameCards.Add(
            new GameCard { Card = CreateCard(1, "9", 9), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(2, "K", 10), OwnerType = OwnerType.Player }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(3, "J", 10), OwnerType = OwnerType.Dealer }
        );
        game.GameCards.Add(
            new GameCard { Card = CreateCard(4, "Q", 10), OwnerType = OwnerType.Dealer }
        );

        _mockGameRepo.Setup(r => r.GetGameByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _sut.PlayerStandAsync(gameId);

        // Assert
        result.Status.Should().Be(GameStatus.RoundEnd);
        result.Money.Should().Be(900); // Money doesn't change, bet is lost
    }
    #endregion
}
