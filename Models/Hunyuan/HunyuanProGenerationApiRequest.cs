using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

internal sealed class HunyuanProGenerationApiRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prompt { get; init; }

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; init; }

    [JsonPropertyName("enable_pbr")]
    public bool EnablePbr { get; init; }

    [JsonPropertyName("face_count")]
    public int FaceCount { get; init; }

    [JsonPropertyName("generate_type")]
    public required string GenerateType { get; init; }

    [JsonPropertyName("polygon_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolygonType { get; init; }

    [JsonPropertyName("multi_view_images")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<HunyuanMultiViewImageApiInput>? MultiViewImages { get; init; }
}
