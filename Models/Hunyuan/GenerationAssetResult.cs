using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

public sealed record GenerationAssetResult(
    [property: JsonPropertyName("asset")] string Asset,
    [property: JsonPropertyName("asset_type")] string AssetType,
    [property: JsonPropertyName("metadata")] object? Metadata);
