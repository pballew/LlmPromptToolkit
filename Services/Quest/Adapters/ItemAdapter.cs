using System.Text.Json.Serialization;

/// <summary>
/// Represents an item in the quest
/// </summary>
public class ItemAdapter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
