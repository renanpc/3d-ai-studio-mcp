namespace ThreeDAiStudioMcp.Models.Hunyuan;

public sealed record HunyuanGenerationSubmissionResult(
    string Edition,
    string TaskId,
    DateTimeOffset CreatedAt,
    string StatusEndpoint,
    string OutputFormatHint);
