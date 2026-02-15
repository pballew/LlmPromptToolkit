using System.Text.Json.Serialization;

/// <summary>
/// Represents quest rewards
/// </summary>
public class RewardsAdapter
{
    [JsonPropertyName("gold")]
    public int Gold { get; set; }

    [JsonPropertyName("experience")]
    public int Experience { get; set; }
}
