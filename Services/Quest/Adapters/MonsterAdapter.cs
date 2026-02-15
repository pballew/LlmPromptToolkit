using System.Text.Json.Serialization;

/// <summary>
/// Represents a monster in the quest
/// </summary>
public class MonsterAdapter
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
