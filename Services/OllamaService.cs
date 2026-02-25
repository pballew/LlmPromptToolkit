using System.Text.Json;

/// <summary>
/// Client for interacting with Ollama API
/// </summary>
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl = "http://localhost:11434";
    private string _modelName = "glm-5:cloud";
    private string? _apiKey;
    private readonly LoggingService? _loggingService;

    public OllamaService(string baseUrl, string? modelName = null, string? apiKey = null, LoggingService? loggingService = null)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _baseUrl = baseUrl.Trim().TrimEnd('/');
        }

        if (!string.IsNullOrWhiteSpace(modelName))
        {
            _modelName = modelName;
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _apiKey = apiKey;
        }

        _loggingService = loggingService;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Gets the current model name
    /// </summary>
    public string GetModelName() => _modelName;

    /// <summary>
    /// Sets a new model with optional base URL and API key
    /// </summary>
    public void SetModel(string modelName, string? baseUrl = null, string? apiKey = null)
    {
        _modelName = modelName;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _baseUrl = baseUrl.Trim().TrimEnd('/');
        }
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _apiKey = apiKey;
        }
        _loggingService?.Log($"Model changed to '{_modelName}' with base URL: {_baseUrl}", "INFO");
    }

    /// <summary>
    /// Checks if the specified model is loaded and running (for Ollama only)
    /// </summary>
    public async Task<bool> IsModelLoadedAsync(string modelName)
    {
        // For non-localhost APIs, skip the local model check
        if (!_baseUrl.Contains("localhost") && !_baseUrl.Contains("127.0.0.1"))
        {
            _loggingService?.Log($"Using online API - skipping local model check", "INFO");
            return true;
        }

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
    /// Pulls and loads the specified model (for Ollama only)
    /// </summary>
    public async Task<bool> PullModelAsync(string modelName)
    {
        // For non-localhost APIs, models are already available
        if (!_baseUrl.Contains("localhost") && !_baseUrl.Contains("127.0.0.1"))
        {
            _loggingService?.Log($"Using online API - model '{modelName}' is already available", "SUCCESS");
            return true;
        }

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

    /// <summary>
    /// Checks if a model is available at a specific endpoint
    /// </summary>
    public async Task<bool> CheckModelAvailabilityAsync(string baseUrl, string modelName, string? apiKey = null)
    {
        try
        {
            var url = baseUrl.TrimEnd('/');
            // For local endpoints, check if model is loaded
            if (url.Contains("localhost") || url.Contains("127.0.0.1"))
            {
                var response = await _httpClient.GetAsync($"{url}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    var tagsJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(tagsJson);
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
                                    _loggingService?.Log($"✓ Local model '{modelName}' is available", "SUCCESS");
                                    return true;
                                }
                            }
                        }
                    }
                    _loggingService?.Log($"✗ Local model '{modelName}' not found", "WARN");
                    return false;
                }
                return false;
            }
            else
            {
                // For online APIs, just try a request to validate the endpoint and key
                var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/tags");
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                }
                
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _loggingService?.Log($"✓ Cloud model '{modelName}' is available", "SUCCESS");
                    return true;
                }
                else
                {
                    _loggingService?.Log($"✗ Cloud model '{modelName}' unavailable: {response.StatusCode}", "WARN");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService?.Log($"Error checking model availability: {ex.Message}", "WARN");
            return false;
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
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/generate")
            {
                Content = content
            };
            
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            HttpResponseMessage response = await _httpClient.SendAsync(request);
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
            _loggingService?.Log($"Make sure the API server is accessible at {_baseUrl}", "ERROR");
            throw new Exception($"Failed to connect to API server at {_baseUrl}. Please check the URL and API key.", ex);
        }
        catch (Exception ex)
        {
            _loggingService?.Log($"✗ Error: {ex.Message}", "ERROR");
            throw;
        }
    }
}
