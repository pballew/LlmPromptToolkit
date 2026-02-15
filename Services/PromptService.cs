using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Service for loading and saving prompt files
/// </summary>
public class PromptService
{
    /// <summary>
    /// Load a prompt from a file asynchronously
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
    /// Load a prompt from a file synchronously
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
    /// Save a prompt to a file asynchronously
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
    /// Save a prompt to a file synchronously
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
