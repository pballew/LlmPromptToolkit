using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a quest JSON schema for a medieval fantasy RPG
/// </summary>
public class QuestResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("challengeRating")]
    public int ChallengeRating { get; set; }

    [JsonPropertyName("objectives")]
    public ObjectiveAdapter[] Objectives { get; set; } = Array.Empty<ObjectiveAdapter>();

    [JsonPropertyName("rewards")]
    public RewardsAdapter? Rewards { get; set; }
}
