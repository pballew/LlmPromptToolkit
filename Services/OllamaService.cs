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
