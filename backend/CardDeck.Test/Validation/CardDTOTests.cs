using CardDeck.Api.Models.DTOs;
using FluentValidation.TestHelper;

namespace CardDeck.Test.Validation;

public class CardDTOTests
{
    // --- CreateCardDTOValidator tests ---

    private readonly CreateCardDTOValidator _createValidator = new();

    [Fact]
    public void CreateValidator_WhenRankIsNull_ShouldHaveValidationError()
    {
        var model = new CreateCardDTO(null!, 1, []);
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rank).WithErrorMessage("Card rank is required.");
    }

    [Fact]
    public void CreateValidator_WhenRankIsEmpty_ShouldHaveValidationError()
    {
        var model = new CreateCardDTO("", 1, []);
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rank);
    }

    [Fact]
    public void CreateValidator_WhenRankIsTooLong_ShouldHaveValidationError()
    {
        var longRank = new string('a', 17); // 17 characters
        var model = new CreateCardDTO(longRank, 1, []);
        var result = _createValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Rank)
            .WithErrorMessage("Card rank cannot exceed 16 characters.");
    }

    [Fact]
    public void CreateValidator_WhenRankIsValid_ShouldNotHaveValidationError()
    {
        var model = new CreateCardDTO("Ace", 1, []);
        var result = _createValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rank);
    }

    // --- UpdateCardDTOValidator tests ---

    private readonly UpdateCardDTOValidator _updateValidator = new();

    [Fact]
    public void UpdateValidator_WhenRankIsNull_ShouldHaveValidationError()
    {
        var model = new UpdateCardDTO(null!, 1, []);
        var result = _updateValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rank).WithErrorMessage("Card rank is required.");
    }

    [Fact]
    public void UpdateValidator_WhenRankIsEmpty_ShouldHaveValidationError()
    {
        var model = new UpdateCardDTO("", 1, []);
        var result = _updateValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rank);
    }

    [Fact]
    public void UpdateValidator_WhenRankIsTooLong_ShouldHaveValidationError()
    {
        var longRank = new string('a', 17); // 17 characters
        var model = new UpdateCardDTO(longRank, 1, []);
        var result = _updateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Rank)
            .WithErrorMessage("Card rank cannot exceed 16 characters.");
    }

    [Fact]
    public void UpdateValidator_WhenRankIsValid_ShouldNotHaveValidationError()
    {
        var model = new UpdateCardDTO("Ace", 1, []);
        var result = _updateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rank);
    }

    // --- PartialUpdateCardDTOValidator tests ---

    private readonly PartialUpdateCardDTOValidator _partialUpdateValidator = new();

    [Fact]
    public void PartialUpdateValidator_WhenRankIsEmpty_ShouldHaveValidationError()
    {
        var model = new PartialUpdateCardDTO("", null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Rank)
            .WithErrorMessage("Card rank cannot be empty if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenRankIsTooLong_ShouldHaveValidationError()
    {
        var longRank = new string('a', 17); // 17 characters
        var model = new PartialUpdateCardDTO(longRank, null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Rank)
            .WithErrorMessage("Card rank cannot exceed 16 characters if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenRankIsValid_ShouldNotHaveValidationError()
    {
        var model = new PartialUpdateCardDTO("Ace", null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rank);
    }

    [Fact]
    public void PartialUpdateValidator_WhenSuitIdIsLessThanOne_ShouldHaveValidationError()
    {
        var model = new PartialUpdateCardDTO(null, 0, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.SuitId)
            .WithErrorMessage("SuitId must be greater than 0 if provided.");
    }
}
