using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

internal sealed class HunyuanMultiViewImageApiInput
{
    [JsonPropertyName("view_type")]
    public required string ViewType { get; init; }

    [JsonPropertyName("view_image")]
    public required string ViewImage { get; init; }
}
