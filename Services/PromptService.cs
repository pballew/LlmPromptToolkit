using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OllamaClient.Models;

/// <summary>
/// Service for loading and saving prompt files
/// </summary>
public class PromptService
{
    private readonly string _promptsDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "Prompts");

    public PromptService()
    {
        // Ensure prompts directory exists
        if (!Directory.Exists(_promptsDirectory))
        {
            Directory.CreateDirectory(_promptsDirectory);
        }
    }

    /// <summary>
    /// Get the file path for a prompt ID
    /// </summary>
    private string GetPromptFilePath(string promptId)
    {
        return Path.Combine(_promptsDirectory, $"{promptId}.json");
    }

    /// <summary>
    /// Load all prompts from the Data/Prompts directory
    /// </summary>
    public async Task<List<Prompt>> GetAllPromptsAsync()
    {
        try
        {
            var prompts = new List<Prompt>();

            if (!Directory.Exists(_promptsDirectory))
            {
                return prompts;
            }

            var files = Directory.GetFiles(_promptsDirectory, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var prompt = JsonSerializer.Deserialize<Prompt>(json);
                    if (prompt != null)
                    {
                        prompts.Add(prompt);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing prompt from {file}: {ex.Message}");
                }
            }

            return prompts.OrderByDescending(p => p.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading prompts: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get a single prompt by ID
    /// </summary>
    public async Task<Prompt> GetPromptAsync(string promptId)
    {
        try
        {
            var filePath = GetPromptFilePath(promptId);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Prompt not found: {promptId}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var prompt = JsonSerializer.Deserialize<Prompt>(json);

            if (prompt == null)
            {
                throw new Exception($"Failed to deserialize prompt from {filePath}");
            }

            return prompt;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading prompt {promptId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save a prompt object as JSON
    /// </summary>
    public async Task SavePromptAsync(Prompt prompt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prompt.Id))
            {
                prompt.Id = Guid.NewGuid().ToString();
            }

            prompt.UpdatedAt = DateTime.Now;
            var filePath = GetPromptFilePath(prompt.Id);

            var json = JsonSerializer.Serialize(prompt, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving prompt: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Delete a prompt by ID
    /// </summary>
    public async Task DeletePromptAsync(string promptId)
    {
        try
        {
            var filePath = GetPromptFilePath(promptId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await Task.CompletedTask;
            }
            else
            {
                throw new FileNotFoundException($"Prompt not found: {promptId}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting prompt {promptId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Load a prompt from a file asynchronously (legacy - works with raw text files)
    /// </summary>
    /// <param name="filePath">The path to the prompt file</param>
    /// <returns>The prompt content as a string</returns>
    public async Task<string> LoadPromptAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Prompt file not found: {filePath}");
            }

            string prompt = await File.ReadAllTextAsync(filePath);
            return prompt;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading prompt from {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Load a prompt from a file synchronously (legacy - works with raw text files)
    /// </summary>
    /// <param name="filePath">The path to the prompt file</param>
    /// <returns>The prompt content as a string</returns>
    public string LoadPrompt(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Prompt file not found: {filePath}");
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading prompt from {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save a prompt to a file asynchronously (legacy - works with raw text files)
    /// </summary>
    /// <param name="filePath">The path where the prompt file should be saved</param>
    /// <param name="prompt">The prompt content to save</param>
    public async Task SavePromptAsync(string filePath, string prompt)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, prompt);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving prompt to {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save a prompt to a file synchronously (legacy - works with raw text files)
    /// </summary>
    /// <param name="filePath">The path where the prompt file should be saved</param>
    /// <param name="prompt">The prompt content to save</param>
    public void SavePrompt(string filePath, string prompt)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, prompt);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving prompt to {filePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if a prompt file exists
    /// </summary>
    /// <param name="filePath">The path to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    public bool PromptExists(string filePath)
    {
        return File.Exists(filePath);
    }
}
