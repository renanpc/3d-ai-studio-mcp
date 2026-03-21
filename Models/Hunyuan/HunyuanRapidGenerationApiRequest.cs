using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

internal sealed class HunyuanRapidGenerationApiRequest
{
    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prompt { get; init; }

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; init; }

    [JsonPropertyName("enable_pbr")]
    public bool EnablePbr { get; init; }

    [JsonPropertyName("enable_geometry")]
    public bool EnableGeometry { get; init; }
}
