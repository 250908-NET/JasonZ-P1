using CardDeck.Api.Models.DTOs;
using FluentValidation.TestHelper;

namespace CardDeck.Test.Validation;

public class AvailableCardDtoValidatorTests
{
    private readonly CreateAvailableCardDTOValidator _createValidator = new();

    [Fact]
    public void CreateValidator_WhenCardIdIsZero_ShouldHaveValidationError()
    {
        var model = new CreateAvailableCardDTO(0);
        var result = _createValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.CardId)
            .WithErrorMessage("Valid CardId is required.");
    }

    [Fact]
    public void CreateValidator_WhenCardIdIsNegative_ShouldHaveValidationError()
    {
        var model = new CreateAvailableCardDTO(-1);
        var result = _createValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.CardId)
            .WithErrorMessage("Valid CardId is required.");
    }

    [Fact]
    public void CreateValidator_WhenCardIdIsPositive_ShouldNotHaveValidationError()
    {
        var model = new CreateAvailableCardDTO(1);
        var result = _createValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CardId);
    }
}
