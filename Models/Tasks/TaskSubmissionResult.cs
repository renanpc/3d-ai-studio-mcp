namespace ThreeDAiStudioMcp.Models.Tasks;

public sealed record TaskSubmissionResult(
    string Operation,
    string TaskId,
    DateTimeOffset CreatedAt,
    string StatusEndpoint,
    string OutputFormatHint);
