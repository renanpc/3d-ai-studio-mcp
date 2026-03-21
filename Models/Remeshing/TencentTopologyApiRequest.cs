using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Remeshing;

internal sealed class TencentTopologyApiRequest
{
    [JsonPropertyName("file_url")]
    public required string FileUrl { get; init; }

    [JsonPropertyName("file_type")]
    public required string FileType { get; init; }

    [JsonPropertyName("polygon_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PolygonType { get; init; }

    [JsonPropertyName("face_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FaceLevel { get; init; }
}
