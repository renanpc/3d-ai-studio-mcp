using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Images;

internal sealed class SeedreamImageGenerationApiRequest
{
    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }

    [JsonPropertyName("image_size")]
    public required string ImageSize { get; init; }

    [JsonPropertyName("num_images")]
    public int NumImages { get; init; }

    [JsonPropertyName("seed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Seed { get; init; }

    [JsonPropertyName("enable_safety_checker")]
    public bool EnableSafetyChecker { get; init; }
}
