using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Trellis;

internal sealed class TrellisGenerationApiRequest
{
    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; init; }

    [JsonPropertyName("image_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("resolution")]
    public required string Resolution { get; init; }

    [JsonPropertyName("steps")]
    public int Steps { get; init; }

    [JsonPropertyName("textures")]
    public bool Textures { get; init; }

    [JsonPropertyName("texture_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TextureSize { get; init; }

    [JsonPropertyName("decimation_target")]
    public int DecimationTarget { get; init; }

    [JsonPropertyName("seed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Seed { get; init; }

    [JsonPropertyName("generate_thumbnail")]
    public bool GenerateThumbnail { get; init; }
}
