using ClinicalTrialApp.Common.Results;
using ClinicalTrialApp.Features.ClinicalTrials.Commands;
using ClinicalTrialApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace Application.Unit.Tests.Features.ClinicalTrials.Commands;

public class UploadTrialDataCommandHandlerTests
{
    private readonly Mock<IClinicalTrialService> _trialServiceMock;
    private readonly Mock<ILogger<UploadTrialDataCommandHandler>> _loggerMock;
    private readonly UploadTrialDataCommandHandler _handler;

    public UploadTrialDataCommandHandlerTests()
    {
        _trialServiceMock = new Mock<IClinicalTrialService>();
        _loggerMock = new Mock<ILogger<UploadTrialDataCommandHandler>>();
        _handler = new UploadTrialDataCommandHandler(_trialServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileContent = "valid json content";
        var fileMock = new Mock<IFormFile>();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        
        fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
        var command = new UploadTrialDataCommand(fileMock.Object);
        
        var expectedId = Guid.NewGuid();
        _trialServiceMock
            .Setup(x => x.ProcessTrialDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(expectedId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value);
    }

    [Fact]
    public async Task Handle_ServiceFailure_ReturnsFailure()
    {
        // Arrange
        var fileContent = "invalid json content";
        var fileMock = new Mock<IFormFile>();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        
        fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
        var command = new UploadTrialDataCommand(fileMock.Object);
        
        var errorMessage = "Processing failed";
        _trialServiceMock
            .Setup(x => x.ProcessTrialDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure(errorMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailure()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Throws(new Exception("Test exception"));
        var command = new UploadTrialDataCommand(fileMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Test exception", result.Error);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
} 