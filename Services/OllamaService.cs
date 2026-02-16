using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Client for interacting with Ollama API
/// </summary>
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:11434";
    private string _modelName = "llama3.1:8b";

    public OllamaService(string baseUrl, string? modelName = null)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _baseUrl = baseUrl.Trim().TrimEnd('/');
        }

        if (!string.IsNullOrWhiteSpace(modelName))
        {
            _modelName = modelName;
        }

        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Gets the current model name
    /// </summary>
    public string GetModelName() => _modelName;

    /// <summary>
    /// Checks if the specified model is loaded and running
    /// </summary>
    public async Task<bool> IsModelLoadedAsync(string modelName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            if (!response.IsSuccessStatusCode)
                return false;

            string tagsJson = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(tagsJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("models", out var modelsArray))
            {
                foreach (var model in modelsArray.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var nameElement))
                    {
                        string name = nameElement.GetString() ?? string.Empty;
                        if (name.StartsWith(modelName))
                            return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if model is loaded: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Pulls and loads the specified model
    /// </summary>
    public async Task<bool> PullModelAsync(string modelName)
    {
        try
        {
            Console.WriteLine($"Pulling model: {modelName}...");

            var request = new { name = modelName };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/pull", content);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Model {modelName} successfully loaded");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to pull model: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error pulling model: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ensures the model is loaded before generating a response
    /// </summary>
    public async Task EnsureModelIsLoadedAsync(string modelName)
    {
        if (!await IsModelLoadedAsync(modelName))
        {
            Console.WriteLine($"Model {modelName} is not loaded. Attempting to pull it...");
            await PullModelAsync(modelName);
        }
    }

    public async Task<LlmResponse> GetLlmResponseAsync(string prompt, int[]? context = null)
    {
        var startTime = DateTime.Now;

        var requestBody = new LlmRequest
        {
            Model = _modelName,
            Prompt = prompt,
            Context = context ?? [],
            Stream = false
        };

        Console.WriteLine("=======================================================================");
        Console.WriteLine($"Model: {requestBody.Model}");
        Console.WriteLine($"Context count: {requestBody.Context.Length}");
        Console.WriteLine($"Prompt: {requestBody.Prompt}");

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            var endTime = DateTime.Now;
            TimeSpan elapsed = endTime - startTime;
            Console.WriteLine($"Request/Response Time: {elapsed.TotalSeconds:F2} seconds");

            var llmResponse = JsonSerializer.Deserialize<LlmResponse>(responseJson);

            llmResponse?.Print();

            Console.WriteLine("=======================================================================\n\n");
            return llmResponse ?? new LlmResponse { Response = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to connect to Ollama server at {_baseUrl}. Make sure Ollama is running.", ex);
        }
    }
}
