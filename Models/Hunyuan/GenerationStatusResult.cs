using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

public sealed record GenerationStatusResult(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("progress")] int Progress,
    [property: JsonPropertyName("failure_reason")] string? FailureReason,
    [property: JsonPropertyName("results")] IReadOnlyList<GenerationAssetResult>? Results);
