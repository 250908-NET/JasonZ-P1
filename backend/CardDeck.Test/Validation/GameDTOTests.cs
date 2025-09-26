using CardDeck.Api.Models.DTOs;
using FluentValidation.TestHelper;

namespace CardDeck.Test.Validation;

public class GameDTOTests
{
    #region StartGameDTOValidator tests
    private readonly StartGameDTOValidator _startGameValidator = new();

    [Fact]
    public void StartGameValidator_WhenTargetIsInvalid_ShouldHaveValidationError()
    {
        var model = new StartGameDTO(Target: 0.5m);
        var result = _startGameValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Target)
            .WithErrorMessage("Target must be at least 1.0.");
    }

    [Fact]
    public void StartGameValidator_WhenDrawLimitIsInvalid_ShouldHaveValidationError()
    {
        var model = new StartGameDTO(DrawLimit: 0);
        var result = _startGameValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.DrawLimit)
            .WithErrorMessage("Draw limit must be at least 1.");
    }

    [Fact]
    public void StartGameValidator_WhenInitialMoneyIsInvalid_ShouldHaveValidationError()
    {
        var model = new StartGameDTO(InitialMoney: -100m);
        var result = _startGameValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.InitialMoney)
            .WithErrorMessage("Initial money cannot be negative.");
    }

    [Fact]
    public void StartGameValidator_WhenAllPropertiesAreNull_ShouldNotHaveValidationErrors()
    {
        var model = new StartGameDTO();
        var result = _startGameValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void StartGameValidator_WhenAllPropertiesAreValid_ShouldNotHaveValidationErrors()
    {
        var model = new StartGameDTO(Target: 21, DrawLimit: 5, InitialMoney: 1000);
        var result = _startGameValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
    #endregion

    #region PlaceBetDTOValidator tests
    private readonly PlaceBetDTOValidator _placeBetValidator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void PlaceBetValidator_WhenAmountIsZeroOrLess_ShouldHaveValidationError(decimal amount)
    {
        var model = new PlaceBetDTO(amount);
        var result = _placeBetValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Bet amount must be greater than 0.");
    }

    [Fact]
    public void PlaceBetValidator_WhenAmountIsPositive_ShouldNotHaveValidationError()
    {
        var model = new PlaceBetDTO(50);
        var result = _placeBetValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
    #endregion
}
