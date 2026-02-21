using System.Text.Json.Serialization;
/// <summary>
/// Request model for Ollama API
/// </summary>
public class LlmRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public int[] Context { get; set; } = [];

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.5;
}
