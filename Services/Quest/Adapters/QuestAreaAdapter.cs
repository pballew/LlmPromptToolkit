using System.Text.Json.Serialization;

/// <summary>
/// Represents a quest area or location
/// </summary>
public class QuestAreaAdapter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
