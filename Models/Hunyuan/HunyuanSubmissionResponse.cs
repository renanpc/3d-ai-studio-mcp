using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

internal sealed record HunyuanSubmissionResponse(
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
