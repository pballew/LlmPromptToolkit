using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
/// <summary>
/// Client for interacting with Ollama API
/// </summary>
public class OllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OllamaClient(string baseUrl = "http://localhost:11434")
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Send a prompt to the Ollama model and get the response context
    /// </summary>
    public async Task<GenerateResponse> GenerateAsync(string model, string prompt, int[] context)
    {
        var requestBody = new GenerateRequest
        {
            Model = model,
            Prompt = prompt,
            Context = context,
            Stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GenerateResponse>(responseJson);

            return result ?? new GenerateResponse { Response = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to connect to Ollama server at {_baseUrl}. Make sure Ollama is running.", ex);
        }
    }
}
