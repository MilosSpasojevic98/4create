using ClinicalTrialApp.Common.Schemas;
using NJsonSchema;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using SchemaType = ClinicalTrialApp.Common.Schemas.SchemaType;

namespace ClinicalTrialApp.Services;

public interface IJsonSchemaValidator
{
    Task<(bool IsValid, ICollection<string> Errors)> ValidateAsync(string jsonData, SchemaType schemaType);
}

public class JsonSchemaValidator : IJsonSchemaValidator
{
    private readonly ConcurrentDictionary<SchemaType, JsonSchema> _schemas;
    private const string SchemaResourcePath = "ClinicalTrialApp.Schemas.{0}-schema.json";

    public JsonSchemaValidator()
    {
        _schemas = new ConcurrentDictionary<SchemaType, JsonSchema>();
    }

    public async Task<(bool IsValid, ICollection<string> Errors)> ValidateAsync(string jsonData, SchemaType schemaType)
    {
        try
        {
            var schema = await GetSchemaAsync(schemaType);
            var validationErrors = schema.Validate(jsonData);
            return (!validationErrors.Any(), validationErrors.Select(e => e.ToString()).ToList());
        }
        catch (JsonException ex)
        {
            return (false, new[] { "Invalid JSON format: " + ex.Message });
        }
    }

    private async Task<JsonSchema> GetSchemaAsync(SchemaType schemaType)
    {
        if (_schemas.TryGetValue(schemaType, out var existingSchema))
        {
            return existingSchema;
        }

        var schemaName = schemaType.ToString().ToLower();
        var resourceName = string.Format(SchemaResourcePath, schemaName);

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");

        using var reader = new StreamReader(stream);
        var schemaJson = await reader.ReadToEndAsync();
        var schema = await JsonSchema.FromJsonAsync(schemaJson);

        _schemas.TryAdd(schemaType, schema);
        return schema;
    }
}