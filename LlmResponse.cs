using System;
using System.Text.Json.Serialization;

/// <summary>
/// Response model for Ollama API
/// </summary>
public class LlmResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("context")]
    public int[] Context { get; set; } = [];

    public void Print()
    {
        Console.WriteLine("LlmResponse");
        Console.WriteLine($" - Model: {Model}");
        Console.WriteLine($" - Created At: {CreatedAt}");
        Console.WriteLine($" - Done: {Done}");
        Console.WriteLine($" - Response:\n{Response}");
        Console.WriteLine($" - Context count: {Context.Length}");
    }
}
