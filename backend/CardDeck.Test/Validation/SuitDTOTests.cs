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
        var longName = new string('a', 33); // 33 characters
        var model = new CreateSuitDTO(longName, '♠', 0);
        var result = _createValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 32 characters.");
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
        var longName = new string('a', 33);
        var model = new UpdateSuitDTO(longName, '♠', 0);
        var result = _updateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 32 characters.");
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
        var model = new PartialUpdateSuitDTO("", null, (string?)null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot be empty if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenNameIsTooLong_ShouldHaveValidationError()
    {
        var longName = new string('a', 33);
        var model = new PartialUpdateSuitDTO(longName, null, (string?)null);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Suit name cannot exceed 32 characters if provided.");
    }

    [Fact]
    public void PartialUpdateValidator_WhenNameIsValid_ShouldNotHaveValidationError()
    {
        var model = new PartialUpdateSuitDTO("Spades", null, (string?)null);
        var result = _partialUpdateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("#12345")]
    [InlineData("123456")]
    [InlineData("#GHIJKL")]
    public void PartialUpdateValidator_WhenColorHexIsInvalid_ShouldHaveValidationError(
        string invalidHex
    )
    {
        var model = new PartialUpdateSuitDTO(null, null, invalidHex);
        var result = _partialUpdateValidator.TestValidate(model);
        result
            .ShouldHaveValidationErrorFor(x => x.ColorHex)
            .WithErrorMessage(
                "ColorHex must be a valid 6-digit hex color code (e.g., #RRGGBB) if provided."
            );
    }

    [Theory]
    [InlineData("#000000")]
    [InlineData("#FFFFFF")]
    [InlineData("#ff00aa")]
    public void PartialUpdateValidator_WhenColorHexIsValid_ShouldNotHaveValidationError(
        string validHex
    )
    {
        var model = new PartialUpdateSuitDTO(null, null, validHex);
        var result = _partialUpdateValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ColorHex);
    }
}
