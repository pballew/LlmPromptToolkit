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

    public PromptAdapterService(OllamaService ollamaService, string modelName = "llama3.1:8b")
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

            // Parse the response JSON into a QuestResponse
            var quest = JsonSerializer.Deserialize<QuestResponse>(response.Response);
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
    /// Generate a quest with context from a previous response
    /// </summary>
    public async Task<QuestResponse?> GenerateQuestWithContextAsync(string previousResponse, string prompt)
    {
        string promptWithContext = $"Previous context:\n{previousResponse}\n\n---\n\n{prompt}";
        return await GenerateQuestAsync(promptWithContext);
    }
}
