using ClinicalTrialApp.Common.Results;
using ClinicalTrialApp.Features.ClinicalTrials.Commands;
using ClinicalTrialApp.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Application.Unit.Tests.Features.ClinicalTrials.Commands;

public class UploadTrialDataCommandHandlerTests
{
    private readonly Mock<IClinicalTrialService> _trialServiceMock;
    private readonly UploadTrialDataCommandHandler _handler;

    public UploadTrialDataCommandHandlerTests()
    {
        _trialServiceMock = new Mock<IClinicalTrialService>();
        _handler = new UploadTrialDataCommandHandler(_trialServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var command = new UploadTrialDataCommand(fileMock.Object);
        
        _trialServiceMock
            .Setup(x => x.ProcessTrialDataAsync(
                It.IsAny<Stream>(), 
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _trialServiceMock.Verify(
            x => x.ProcessTrialDataAsync(
                It.IsAny<Stream>(), 
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceFailure_ReturnsFailure()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var command = new UploadTrialDataCommand(fileMock.Object);
        var errorMessage = "Processing failed";
        
        _trialServiceMock
            .Setup(x => x.ProcessTrialDataAsync(
                It.IsAny<Stream>(), 
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(errorMessage));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error);
    }
} 