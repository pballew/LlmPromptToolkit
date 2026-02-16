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
}
