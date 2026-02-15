using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a quest objective
/// </summary>
public class ObjectiveAdapter
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("questArea")]
    public QuestAreaAdapter? QuestArea { get; set; }

    [JsonPropertyName("Npcs")]
    public NpcAdapter[] Npcs { get; set; } = Array.Empty<NpcAdapter>();

    [JsonPropertyName("monsters")]
    public MonsterAdapter[] Monsters { get; set; } = Array.Empty<MonsterAdapter>();

    [JsonPropertyName("item")]
    public ItemAdapter[] Items { get; set; } = Array.Empty<ItemAdapter>();
}
