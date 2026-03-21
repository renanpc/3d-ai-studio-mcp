using System.ComponentModel;

namespace ThreeDAiStudioMcp.Models.Hunyuan;

public sealed class HunyuanMultiViewImageInput
{
    [Description("Camera view type. Model 3.0 supports front, left, right, back. Model 3.1 also supports top, bottom, left_front, and right_front.")]
    public required string ViewType { get; init; }

    [Description("Reference image as a data URI or base64 string. Use this or viewImageFilePath.")]
    public string? ViewImage { get; init; }

    [Description("Optional local file path to an image. The server converts it to a data URI automatically.")]
    public string? ViewImageFilePath { get; init; }
}
