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

    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; set; } = 0;

    [JsonPropertyName("eval_count")]
    public int EvalCount { get; set; } = 0;

    public void Print()
    {
        Console.WriteLine("LlmResponse");
        Console.WriteLine($" - Model: {Model}");
        Console.WriteLine($" - Created At: {CreatedAt}");
        Console.WriteLine($" - Done: {Done}");
        Console.WriteLine($" - Response:\n{Response}");
        Console.WriteLine($" - Context count: {Context.Length}");
        Console.WriteLine($" - Prompt tokens: {PromptEvalCount}, Output tokens: {EvalCount}");
    }
}
