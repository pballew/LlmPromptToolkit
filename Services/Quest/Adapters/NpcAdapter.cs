using System.Text.Json.Serialization;

/// <summary>
/// Represents a non-player character
/// </summary>
public class NpcAdapter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
