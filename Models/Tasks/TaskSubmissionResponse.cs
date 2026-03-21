using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Tasks;

internal sealed record TaskSubmissionResponse(
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
