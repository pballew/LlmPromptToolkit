using System.Text.Json;

/// <summary>
/// Client for interacting with Ollama API
/// </summary>
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:11434";
    private string _modelName = "llama3.1:8b";
    private readonly LoggingService? _loggingService;

    public OllamaService(string baseUrl, string? modelName = null, LoggingService? loggingService = null)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _baseUrl = baseUrl.Trim().TrimEnd('/');
        }

        if (!string.IsNullOrWhiteSpace(modelName))
        {
            _modelName = modelName;
        }

        _loggingService = loggingService;
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
            _loggingService?.Log($"Checking if model '{modelName}' is loaded...", "INFO");
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            if (!response.IsSuccessStatusCode)
            {
                _loggingService?.Log($"Failed to get model tags: {response.StatusCode}", "WARN");
                return false;
            }

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
                        {
                            _loggingService?.Log($"Model '{modelName}' is loaded and ready", "SUCCESS");
                            return true;
                        }
                    }
                }
            }

            _loggingService?.Log($"Model '{modelName}' not found in loaded models", "WARN");
            return false;
        }
        catch (Exception ex)
        {
            _loggingService?.Log($"Error checking if model is loaded: {ex.Message}", "ERROR");
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
            _loggingService?.Log($"🔄 Pulling model: {modelName}...", "INFO");

            var request = new { name = modelName };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/pull", content);
            
            if (response.IsSuccessStatusCode)
            {
                _loggingService?.Log($"✓ Model '{modelName}' successfully loaded", "SUCCESS");
                return true;
            }
            else
            {
                _loggingService?.Log($"✗ Failed to pull model: {response.StatusCode}", "ERROR");
                return false;
            }
        }
        catch (Exception ex)
        {
            _loggingService?.Log($"Error pulling model: {ex.Message}", "ERROR");
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
            _loggingService?.Log($"Model '{modelName}' is not loaded. Attempting to pull it...", "INFO");
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
            Stream = false,
            Temperature = 0.5
        };

        _loggingService?.Log($"═══════════════════════════════════════════════════════════════", "DEBUG");
        _loggingService?.Log($"🤖 Model: {requestBody.Model}", "INFO");
        _loggingService?.Log($"🌡️  Temperature: {requestBody.Temperature}", "INFO");
        _loggingService?.Log($"📝 Context count: {requestBody.Context.Length}", "INFO");
        _loggingService?.Log($"💬 Prompt: {requestBody.Prompt.Substring(0, Math.Min(100, requestBody.Prompt.Length))}...", "INFO");
        _loggingService?.Log($"⏳ Sending request to {_baseUrl}/api/generate", "INFO");

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            _loggingService?.Log("📤 Waiting for response...", "DEBUG");
            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            var endTime = DateTime.Now;
            TimeSpan elapsed = endTime - startTime;
            
            _loggingService?.Log($"✓ Response received in {elapsed.TotalSeconds:F2} seconds", "SUCCESS");

            var llmResponse = JsonSerializer.Deserialize<LlmResponse>(responseJson);

            if (llmResponse != null)
            {
                _loggingService?.Log($"📊 Response length: {llmResponse.Response.Length} characters", "INFO");
                _loggingService?.Log($"🎯 Model used: {llmResponse.Model}", "INFO");
                _loggingService?.Log($"✓ Request completed successfully", "SUCCESS");
            }

            _loggingService?.Log($"═══════════════════════════════════════════════════════════════", "DEBUG");
            
            return llmResponse ?? new LlmResponse { Response = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            _loggingService?.Log($"✗ Connection failed: {ex.Message}", "ERROR");
            _loggingService?.Log($"Make sure Ollama is running at {_baseUrl}", "ERROR");
            throw new Exception($"Failed to connect to Ollama server at {_baseUrl}. Make sure Ollama is running.", ex);
        }
        catch (Exception ex)
        {
            _loggingService?.Log($"✗ Error: {ex.Message}", "ERROR");
            throw;
        }
    }
}
