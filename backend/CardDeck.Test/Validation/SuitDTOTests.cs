using CardDeck.Api.Models.DTOs;
using FluentValidation.TestHelper;

namespace CardDeck.Test.Validation;

public class SuitDtoValidatorTests
{
    // --- CreateSuitDTOValidator tests ---

    private readonly CreateSuitDTOValidator _createValidator = new();

    [Fact]
    public void CreateValidator_WhenNameIsNull_ShouldHaveValidationError()
    {
        var model = new CreateSuitDTO(null!, '♠', 0);
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Suit name is required.");
    }

    [Fact]
    public void CreateValidator_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var model = new CreateSuitDTO("", '♠', 0);
        var result = _createValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateValidator_WhenNameIsTooLong_ShouldHaveValidationError()
    {
        var longName = new string('a', 16); // 16 characters
        var model = new CreateSuitDTO(longName, '♠', 0);
        var result = _createValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 15 characters.");
    }

    [Fact]
    public void CreateValidator_WhenNameIsValid_ShouldNotHaveValidationError()
    {
        var model = new CreateSuitDTO("Spades", '♠', 0);
        var result = _createValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // --- UpdateSuitDTOValidator tests ---

    private readonly UpdateSuitDTOValidator _updateValidator = new();

    [Fact]
    public void UpdateValidator_WhenNameIsNull_ShouldHaveValidationError()
    {
        var model = new UpdateSuitDTO(null!, '♠', 0);
        var result = _updateValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Suit name is required.");
    }

    [Fact]
    public void UpdateValidator_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var model = new UpdateSuitDTO("", '♠', 0);
        var result = _updateValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateValidator_WhenNameIsTooLong_ShouldHaveValidationError()
    {
        var longName = new string('a', 16);
        var model = new UpdateSuitDTO(longName, '♠', 0);
        var result = _updateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 15 characters.");
    }

    [Fact]
    public void UpdateValidator_WhenNameIsValid_ShouldNotHaveValidationError()
    {
        var model = new UpdateSuitDTO("Spades", '♠', 0);
        var result = _updateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // --- PartialUpdateSuitDTOValidator tests ---

    private readonly PartialUpdateSuitDTOValidator _partialUpdateValidator = new();

    [Fact]
    public void PartialUpdateValidator_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var model = new PartialUpdateSuitDTO("", null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot be empty if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenNameIsTooLong_ShouldHaveValidationError()
    {
        var longName = new string('a', 16);
        var model = new PartialUpdateSuitDTO(longName, null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 15 characters if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenNameIsValid_ShouldNotHaveValidationError()
    {
        var model = new PartialUpdateSuitDTO("Spades", null, null);
        var result = _partialUpdateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void PartialUpdateValidator_WhenColorRGBIsOutOfRange_ShouldHaveValidationError()
    {
        var model = new PartialUpdateSuitDTO(null, null, 20000000);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.ColorRGB)
            .WithErrorMessage("ColorRGB must be between 0 and 16777215 if provided.");
    }
}
