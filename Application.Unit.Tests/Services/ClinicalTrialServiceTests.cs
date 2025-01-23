using ClinicalTrialApp.Common.Schemas;
using ClinicalTrialApp.Data;
using ClinicalTrialApp.Models;
using ClinicalTrialApp.Services;
using MediatR;
using Moq;
using System.Text;
using System.Text.Json;

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

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((true, Array.Empty<string>()));

        _unitOfWorkMock
            .Setup(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()))
            .ReturnsAsync(new ClinicalTrialMetadata());

        // Act
        var result = await _service.ProcessTrialDataAsync(stream, validJson.Length);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessTrialDataAsync_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((false, new[] { "Invalid JSON format" }));

        // Act
        var result = await _service.ProcessTrialDataAsync(stream, invalidJson.Length);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid JSON format", result.Error);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
            .ReturnsAsync((true, Array.Empty<string>()));

        ClinicalTrialMetadata capturedTrial = null;
        _unitOfWorkMock
            .Setup(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()))
            .Callback<ClinicalTrialMetadata>(trial => capturedTrial = trial)
            .ReturnsAsync((ClinicalTrialMetadata trial) => trial);

        // Act
        var result = await _service.ProcessTrialDataAsync(stream, validJson.Length);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedTrial);
        Assert.True(capturedTrial.EndDate.HasValue);
        Assert.Equal(30, capturedTrial.DurationInDays);
        Assert.Equal(TrialStatus.Ongoing, capturedTrial.Status);
    }
}