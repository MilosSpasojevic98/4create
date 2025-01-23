using ClinicalTrialApp.Common.Schemas;
using ClinicalTrialApp.Services;

namespace Application.Unit.Tests.Services;

public class JsonSchemaValidatorTests
{
    private readonly IJsonSchemaValidator _validator;

    public JsonSchemaValidatorTests()
    {
        _validator = new JsonSchemaValidator();
    }

    [Fact]
    public async Task ValidateAsync_ValidJson_ReturnsTrue()
    {
        // Arrange
        var validJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""endDate"": ""2024-02-25"",
            ""participants"": 100,
            ""status"": ""Ongoing""
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(validJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_MissingRequiredField_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"{
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing""
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(invalidJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, error => error.Contains("trialId"));
    }

    [Fact]
    public async Task ValidateAsync_InvalidDateFormat_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""invalid-date"",
            ""status"": ""Ongoing""
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(invalidJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, error => error.Contains("startDate"));
    }

    [Fact]
    public async Task ValidateAsync_InvalidStatus_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Invalid""
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(invalidJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, error => error.Contains("status"));
    }

    [Fact]
    public async Task ValidateAsync_InvalidParticipantsValue_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing"",
            ""participants"": 0
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(invalidJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, error => error.Contains("participants"));
    }

    [Fact]
    public async Task ValidateAsync_AdditionalProperties_ReturnsFalse()
    {
        // Arrange
        var invalidJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing"",
            ""unknownProperty"": ""value""
        }";

        // Act
        var (isValid, errors) = await _validator.ValidateAsync(invalidJson, SchemaType.ClinicalTrial);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, error => error.Contains("NoAdditionalPropertiesAllowed"));
    }

    [Fact]
    public async Task ValidateAsync_InvalidSchemaType_ThrowsException()
    {
        // Arrange
        var validJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing""
        }";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _validator.ValidateAsync(validJson, (SchemaType)999));
    }
}