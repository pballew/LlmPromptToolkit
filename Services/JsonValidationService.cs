using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Service for validating JSON responses from Ollama
/// </summary>
public class JsonValidationService
{
    /// <summary>
    /// Validates a JSON response and checks required fields
    /// </summary>
    public async Task<ValidationResult> ValidateJsonAsync(string responseText, IEnumerable<string>? requiredFields = null)
    {
        var result = new ValidationResult();
        requiredFields = requiredFields ?? new List<string>();

        // Try to parse as JSON
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                // JSON is valid
                result.IsValid = true;
                result.Errors.Clear();

                // Check required fields if provided
                if (requiredFields != null)
                {
                    CheckRequiredFields(doc.RootElement, requiredFields, result);
                }
            }
        }
        catch (JsonException ex)
        {
            // JSON parse error
            result.IsValid = false;
            result.ParseError = ex.Message;
            result.Errors.Add($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Other errors
            result.IsValid = false;
            result.ParseError = ex.Message;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Validates a response as plain text (JSON or markup)
    /// </summary>
    public async Task<ValidationResult> ValidateResponseAsync(string responseText, IEnumerable<string>? requiredFields = null)
    {
        var result = new ValidationResult();
        requiredFields = requiredFields ?? new List<string>();

        // First, check if it's valid JSON
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                result.IsValid = true;

                // Check required fields if provided
                if (requiredFields != null)
                {
                    CheckRequiredFields(doc.RootElement, requiredFields, result);
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON, but might still be valid response
            result.IsValid = true;
            result.ParseError = "Response is not JSON (plain text response is OK)";

            // Check for required field keywords in text
            if (requiredFields != null)
            {
                CheckRequiredFieldsInText(responseText, requiredFields, result);
            }
        }

        return await Task.FromResult(result);
    }

    private void CheckRequiredFields(JsonElement root, IEnumerable<string> requiredFields, ValidationResult result)
    {
        var requiredList = new List<string>(requiredFields);
        result.TotalRequiredFields = requiredList.Count;
        result.PopulatedRequiredFields = 0;

        foreach (var field in requiredList)
        {
            bool exists = false;
            bool populated = false;

            if (root.TryGetProperty(field, out var element))
            {
                exists = true;
                // Check if field has a value
                if (element.ValueKind != JsonValueKind.Null)
                {
                    populated = !string.IsNullOrWhiteSpace(element.GetRawText());
                }
            }

            result.RequiredFieldsStatus[field] = populated;

            if (populated)
            {
                result.PopulatedRequiredFields++;
            }
            else if (exists)
            {
                result.Errors.Add($"Required field '{field}' is empty or null");
                result.IsValid = false;
            }
            else
            {
                result.Errors.Add($"Required field '{field}' is missing");
                result.IsValid = false;
            }
        }
    }

    private void CheckRequiredFieldsInText(string text, IEnumerable<string> requiredFields, ValidationResult result)
    {
        var requiredList = new List<string>(requiredFields);
        result.TotalRequiredFields = requiredList.Count;
        result.PopulatedRequiredFields = 0;

        foreach (var field in requiredList)
        {
            bool found = text.Contains(field, StringComparison.OrdinalIgnoreCase);
            result.RequiredFieldsStatus[field] = found;

            if (found)
            {
                result.PopulatedRequiredFields++;
            }
            else
            {
                result.Errors.Add($"Required field '{field}' not found in response");
            }
        }
    }

    /// <summary>
    /// Validates a response against a JSON schema
    /// </summary>
    public async Task<(bool isValid, List<string> errors)> ValidateAgainstSchemaAsync(string responseJson, string schemaJson)
    {
        var errors = new List<string>();

        try
        {
            // Parse response
            using JsonDocument responseDoc = JsonDocument.Parse(responseJson);
            using JsonDocument schemaDoc = JsonDocument.Parse(schemaJson);

            var responseElement = responseDoc.RootElement;
            var schemaElement = schemaDoc.RootElement;

            // Validate against schema
            ValidateElementAgainstSchema(responseElement, schemaElement, "", errors);

            return await Task.FromResult((errors.Count == 0, errors));
        }
        catch (JsonException ex)
        {
            errors.Add($"JSON Parse Error: {ex.Message}");
            return await Task.FromResult((false, errors));
        }
        catch (Exception ex)
        {
            errors.Add($"Schema validation error: {ex.Message}");
            return await Task.FromResult((false, errors));
        }
    }

    private void ValidateElementAgainstSchema(JsonElement element, JsonElement schema, string path, List<string> errors)
    {
        // Get required properties from schema
        if (schema.TryGetProperty("required", out var requiredElement) && requiredElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var requiredProp in requiredElement.EnumerateArray())
            {
                string propName = requiredProp.GetString() ?? "";
                if (!element.TryGetProperty(propName, out _))
                {
                    errors.Add($"Missing required field: '{propName}'");
                }
            }
        }

        // Validate properties
        if (schema.TryGetProperty("properties", out var propertiesElement) && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in propertiesElement.EnumerateObject())
            {
                string propName = property.Name;
                if (element.TryGetProperty(propName, out var propValue))
                {
                    // Basic type validation
                    if (property.Value.TryGetProperty("type", out var typeElement))
                    {
                        string expectedType = typeElement.GetString() ?? "";
                        string actualType = GetJsonTypeName(propValue);

                        if (!TypeMatches(expectedType, actualType))
                        {
                            errors.Add($"Field '{propName}' has type '{actualType}' but expected '{expectedType}'");
                        }
                    }
                }
            }
        }
    }

    private string GetJsonTypeName(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => "object",
            JsonValueKind.Array => "array",
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Null => "null",
            _ => "unknown"
        };
    }

    private bool TypeMatches(string schemaType, string jsonType)
    {
        if (schemaType == jsonType) return true;
        
        // Allow type variations
        if (schemaType == "boolean" && jsonType == "boolean") return true;
        if ((schemaType == "integer" || schemaType == "number") && jsonType == "number") return true;
        
        return false;
    }
}
