using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Texturing;

internal sealed class TencentTextureEditApiRequest
{
    [JsonPropertyName("file_url")]
    public required string FileUrl { get; init; }

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; init; }

    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prompt { get; init; }

    [JsonPropertyName("enable_pbr")]
    public bool EnablePbr { get; init; }
}
