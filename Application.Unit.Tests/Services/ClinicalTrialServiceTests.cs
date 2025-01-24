using ClinicalTrialApp.Common.Schemas;
using ClinicalTrialApp.Data;
using ClinicalTrialApp.Models;
using ClinicalTrialApp.Services;
using MediatR;
using Moq;
using System.Text.Json;
using System.Linq.Expressions;

namespace Application.Unit.Tests.Services;

public class ClinicalTrialServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJsonSchemaValidator> _validatorMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly ClinicalTrialService _service;

    public ClinicalTrialServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IJsonSchemaValidator>();
        _publisherMock = new Mock<IPublisher>();
        _service = new ClinicalTrialService(_unitOfWorkMock.Object, _validatorMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task ProcessTrialDataAsync_ValidData_ReturnsSuccess()
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

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((true, Array.Empty<string>()));

        _unitOfWorkMock.Setup(x => x.ClinicalTrials.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClinicalTrialMetadata, bool>>>()))
            .ReturnsAsync((ClinicalTrialMetadata)null);

        ClinicalTrialMetadata capturedTrial = null;
        _unitOfWorkMock
            .Setup(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()))
            .Callback<ClinicalTrialMetadata>(trial => capturedTrial = trial)
            .ReturnsAsync((ClinicalTrialMetadata trial) => trial);

        // Act
        var result = await _service.ProcessTrialDataAsync(validJson);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessTrialDataAsync_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((false, new[] { "Invalid JSON format" }));

        // Act
        var result = await _service.ProcessTrialDataAsync(invalidJson);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid JSON format", result.Error);
        _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTrialDataAsync_DuplicateTrialId_ReturnsFailure()
    {
        // Arrange
        var validJson = @"{
            ""trialId"": ""TEST-001"",
            ""title"": ""Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing""
        }";

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((true, Array.Empty<string>()));

        var existingTrial = JsonSerializer.Deserialize<ClinicalTrialMetadata>(validJson);
        _unitOfWorkMock.Setup(x => x.ClinicalTrials.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClinicalTrialMetadata, bool>>>()))
            .ReturnsAsync(existingTrial);

        // Act
        var result = await _service.ProcessTrialDataAsync(validJson);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("has already been uploaded", result.Error);
        _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTrialDataAsync_ValidatesBusinessRules()
    {
        // Arrange
        var validJson = @"{
            ""trialId"": ""TEST-002"",
            ""title"": ""Business Rules Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""status"": ""Ongoing""
        }";

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((true, Array.Empty<string>()));

        _unitOfWorkMock.Setup(x => x.ClinicalTrials.FirstOrDefaultAsync(It.IsAny<Expression<Func<ClinicalTrialMetadata, bool>>>()))
            .ReturnsAsync((ClinicalTrialMetadata)null);

        ClinicalTrialMetadata capturedTrial = null;
        _unitOfWorkMock
            .Setup(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()))
            .Callback<ClinicalTrialMetadata>(trial => capturedTrial = trial)
            .ReturnsAsync((ClinicalTrialMetadata trial) => trial);

        // Act
        var result = await _service.ProcessTrialDataAsync(validJson);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedTrial);
        Assert.True(capturedTrial.EndDate.HasValue);
        Assert.Equal(31, capturedTrial.DurationInDays);
        Assert.Equal(TrialStatus.Ongoing, capturedTrial.Status);
    }
}