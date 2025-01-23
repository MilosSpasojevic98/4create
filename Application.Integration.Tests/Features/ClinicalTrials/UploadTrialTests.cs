using System.Net;
using System.Text;
using ClinicalTrialApp.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Application.Integration.Tests.Features.ClinicalTrials;

public class UploadTrialTests : IntegrationTestBase
{
    public UploadTrialTests(IntegrationTestWebAppFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task Upload_ValidTrial_ReturnsSuccess()
    {
        // Arrange
        var validJson = @"{
            ""trialId"": ""INT-TEST-001"",
            ""title"": ""Integration Test Trial"",
            ""startDate"": ""2024-01-25"",
            ""endDate"": ""2024-02-25"",
            ""participants"": 100,
            ""status"": ""Ongoing""
        }";

        var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(validJson));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        formData.Add(fileContent, "file", "trial.json");

        // Act
        var response = await Client.PostAsync("/api/trials/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the trial was saved
        var savedTrial = await DbContext.ClinicalTrialData
            .FirstOrDefaultAsync(t => t.TrialId == "INT-TEST-001");
        
        Assert.NotNull(savedTrial);
        Assert.Equal("Integration Test Trial", savedTrial.Title);
        Assert.Equal(100, savedTrial.Participants);
        Assert.Equal(TrialStatus.Ongoing, savedTrial.Status);
        Assert.True(savedTrial.EndDate.HasValue);
        Assert.Equal(31, savedTrial.DurationInDays);
    }

    [Fact]
    public async Task Upload_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = @"{
            ""title"": ""Missing Required Fields Trial"",
            ""status"": ""Ongoing""
        }";

        var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(invalidJson));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        formData.Add(fileContent, "file", "invalid.json");

        // Act
        var response = await Client.PostAsync("/api/trials/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(invalidJson));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        formData.Add(fileContent, "file", "invalid.json");

        // Act
        var response = await Client.PostAsync("/api/trials/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_NonJsonFile_ReturnsBadRequest()
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("not json"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        formData.Add(fileContent, "file", "test.txt");

        // Act
        var response = await Client.PostAsync("/api/trials/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 