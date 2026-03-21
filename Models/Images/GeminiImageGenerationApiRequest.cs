using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Images;

internal sealed class GeminiImageGenerationApiRequest
{
    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }

    [JsonPropertyName("output_format")]
    public required string OutputFormat { get; init; }

    [JsonPropertyName("aspect_ratio")]
    public required string AspectRatio { get; init; }

    [JsonPropertyName("resolution")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Resolution { get; init; }

    [JsonPropertyName("num_images")]
    public int NumImages { get; init; }
}
