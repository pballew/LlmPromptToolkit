using System;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Service for handling prompt generation and adapter conversion
/// </summary>
public class PromptAdapterService
{
    private readonly OllamaService _ollamaService;
    private readonly string _modelName;

    public PromptAdapterService(OllamaService ollamaService, string modelName = "glm-5:cloud")
    {
        _ollamaService = ollamaService;
        _modelName = modelName;
    }

    /// <summary>
    /// Generate a quest from a prompt and return it as a QuestResponse object
    /// </summary>
    public async Task<QuestResponse?> GenerateQuestAsync(string prompt)
    {
        try
        {
            var response = await _ollamaService.GetLlmResponseAsync(prompt);
            
            if (string.IsNullOrEmpty(response.Response))
            {
                Console.WriteLine("No response received from the model");
                return null;
            }

            // Try to extract JSON from the response
            string jsonText = ExtractJsonFromResponse(response.Response);
            
            if (string.IsNullOrEmpty(jsonText))
            {
                Console.WriteLine("No JSON found in response");
                return null;
            }

            // Parse the extracted JSON into a QuestResponse
            var quest = JsonSerializer.Deserialize<QuestResponse>(jsonText);
            return quest;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse response as JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating quest: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Extract JSON from response text that may contain narrative or markdown code blocks
    /// </summary>
    private string? ExtractJsonFromResponse(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        // Try pattern: json: ``` {...} ```
        var jsonPrefixMatch = System.Text.RegularExpressions.Regex.Match(text, @"json:\s*```\s*([\s\S]*?)```");
        if (jsonPrefixMatch.Success)
        {
            var jsonText = jsonPrefixMatch.Groups[1].Value.Trim();
            if (TryValidateJson(jsonText))
                return jsonText;
        }

        // Try pattern: ```json {...} ``` or ``` {...} ```
        var jsonMatch = System.Text.RegularExpressions.Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)```");
        if (jsonMatch.Success)
        {
            var jsonText = jsonMatch.Groups[1].Value.Trim();
            if (TryValidateJson(jsonText))
                return jsonText;
        }

        // Try to find JSON object starting with { and ending with }
        int startIdx = text.IndexOf('{');
        if (startIdx >= 0)
        {
            int braceCount = 0;
            int endIdx = -1;
            
            for (int i = startIdx; i < text.Length; i++)
            {
                if (text[i] == '{')
                    braceCount++;
                else if (text[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        endIdx = i;
                        break;
                    }
                }
            }

            if (endIdx > startIdx)
            {
                var jsonText = text.Substring(startIdx, endIdx - startIdx + 1);
                if (TryValidateJson(jsonText))
                    return jsonText;
            }
        }

        // If nothing worked, return the text as-is (might be pure JSON)
        if (TryValidateJson(text.Trim()))
            return text.Trim();

        return null;
    }

    /// <summary>
    /// Check if a string is valid JSON by attempting to parse it
    /// </summary>
    private bool TryValidateJson(string jsonText)
    {
        try
        {
            using (JsonDocument.Parse(jsonText)) { }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate a quest with context from a previous response
    /// </summary>
    public async Task<QuestResponse?> GenerateQuestWithContextAsync(string previousResponse, string prompt)
    {
        string promptWithContext = $"Previous context:\n{previousResponse}\n\n---\n\n{prompt}";
        return await GenerateQuestAsync(promptWithContext);
    }
}
