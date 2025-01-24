using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ClinicalTrialApp.Services;
using ClinicalTrialApp.Core.Entities;
using ClinicalTrialApp.Core.Interfaces;
using ClinicalTrialApp.Core.Enums;
using ClinicalTrialApp.Core.Results;
using ClinicalTrialApp.Core.Services;
using ClinicalTrialApp.Core.Publishers;
using System.Linq.Expressions;

namespace ClinicalTrialApp.Tests.Services
{
    public class ClinicalTrialServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IJsonSchemaValidator> _schemaValidatorMock;
        private readonly Mock<IPublisher> _publisherMock;
        private readonly ClinicalTrialService _service;

        public ClinicalTrialServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _schemaValidatorMock = new Mock<IJsonSchemaValidator>();
            _publisherMock = new Mock<IPublisher>();
            _service = new ClinicalTrialService(_unitOfWorkMock.Object, _schemaValidatorMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task ProcessTrialDataAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var jsonContent = GetValidJsonContent();
            _schemaValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _service.ProcessTrialDataAsync(jsonContent);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTrialDataAsync_WithInvalidSchema_ReturnsFailure()
        {
            // Arrange
            var jsonContent = GetInvalidJsonContent();
            var errors = new[] { "Invalid schema" };
            _schemaValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _service.ProcessTrialDataAsync(jsonContent);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Invalid JSON format", result.Error);
            _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTrialDataAsync_WithInvalidJson_ReturnsFailure()
        {
            // Arrange
            var jsonContent = "invalid json";
            _schemaValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _service.ProcessTrialDataAsync(jsonContent);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("JSON parsing error", result.Error);
            _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTrialDataAsync_WithDuplicateTrialId_ReturnsFailure()
        {
            // Arrange
            var jsonContent = GetValidJsonContent();
            _schemaValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<string>(), SchemaType.ClinicalTrial))
                .ReturnsAsync((true, Array.Empty<string>()));

            var existingTrial = JsonSerializer.Deserialize<ClinicalTrialMetadata>(jsonContent);
            _unitOfWorkMock.Setup(x => x.ClinicalTrials.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<ClinicalTrialMetadata, bool>>>()))
                .ReturnsAsync(existingTrial);

            // Act
            var result = await _service.ProcessTrialDataAsync(jsonContent);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("has already been uploaded", result.Error);
            _unitOfWorkMock.Verify(x => x.ClinicalTrials.AddAsync(It.IsAny<ClinicalTrialMetadata>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static string GetValidJsonContent()
        {
            var trial = new ClinicalTrialMetadata
            {
                TrialId = "TEST001",
                Title = "Test Trial",
                StartDate = DateTime.UtcNow,
                Status = TrialStatus.Ongoing
            };

            return JsonSerializer.Serialize(trial);
        }

        private static string GetInvalidJsonContent()
        {
            return "{ invalid: json }";
        }
    }
} 