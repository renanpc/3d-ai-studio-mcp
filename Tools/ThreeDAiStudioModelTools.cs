using System.ComponentModel;
using ModelContextProtocol.Server;
using ThreeDAiStudioMcp.Clients;
using ThreeDAiStudioMcp.Models.Api;
using ThreeDAiStudioMcp.Models.Remeshing;
using ThreeDAiStudioMcp.Models.Tasks;
using ThreeDAiStudioMcp.Models.Texturing;
using ThreeDAiStudioMcp.Models.Trellis;
using ThreeDAiStudioMcp.Utilities;

namespace ThreeDAiStudioMcp.Tools;

internal sealed class ThreeDAiStudioModelTools(ThreeDAiStudioApiClient apiClient)
{
    private static readonly HashSet<string> SupportedTrellisResolutions = new(StringComparer.OrdinalIgnoreCase)
    {
        "512",
        "1024",
        "1536"
    };

    private static readonly HashSet<int> SupportedTrellisTextureSizes =
    [
        1024,
        2048,
        4096
    ];

    private static readonly HashSet<string> SupportedRemeshFileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "GLB",
        "OBJ"
    };

    private static readonly HashSet<string> SupportedPolygonTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "triangle",
        "quadrilateral"
    };

    private static readonly HashSet<string> SupportedFaceLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "high",
        "medium",
        "low"
    };

    [McpServerTool(Name = "generate_trellis2_model")]
    [Description("Submit a TRELLIS.2 image-to-3D generation request to 3D AI Studio.")]
    public async Task<TaskSubmissionResult> GenerateTrellis2Model(
        [Description("Reference image as a data URI or base64 string. Use this, imageFilePath, or imageUrl.")] string? image = null,
        [Description("Optional local file path to a reference image. The server converts it to a data URI automatically.")] string? imageFilePath = null,
        [Description("Optional public URL to the reference image. Use this or a local/base64 image, but not both.")] string? imageUrl = null,
        [Description("Voxel resolution: 512, 1024, or 1536.")] string resolution = "1024",
        [Description("Sampling steps from 1 to 50.")] int steps = 12,
        [Description("Generate a fully textured PBR GLB instead of geometry only.")] bool textures = false,
        [Description("Texture resolution when textures=true: 1024, 2048, or 4096.")] int? textureSize = null,
        [Description("Target face count for simplification from 1000 to 16000000.")] int decimationTarget = 1000000,
        [Description("Optional seed for reproducible output.")] int? seed = null,
        [Description("Generate an extra 400x400 thumbnail preview.")] bool generateThumbnail = false)
    {
        try
        {
            var resolvedImage = ImageInputResolver.ResolveOptionalImage(image, imageFilePath, nameof(image));
            var normalizedImageUrl = NormalizeOptionalValue(imageUrl);
            var normalizedResolution = NormalizeRequiredValue(resolution, nameof(resolution));

            ValidateTrellisRequest(
                resolvedImage,
                normalizedImageUrl,
                normalizedResolution,
                steps,
                textures,
                textureSize,
                decimationTarget,
                seed);

            var response = await apiClient.SubmitTaskAsync(
                "v1/3d-models/trellis2/generate/",
                new TrellisGenerationApiRequest
                {
                    Image = resolvedImage,
                    ImageUrl = normalizedImageUrl,
                    Resolution = normalizedResolution,
                    Steps = steps,
                    Textures = textures,
                    TextureSize = textures ? textureSize ?? 2048 : null,
                    DecimationTarget = decimationTarget,
                    Seed = seed,
                    GenerateThumbnail = generateThumbnail
                });

            return new TaskSubmissionResult(
                Operation: "TRELLIS.2",
                TaskId: response.TaskId,
                CreatedAt: response.CreatedAt,
                StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
                OutputFormatHint: textures
                    ? "When finished, TRELLIS.2 returns a textured GLB with asset_type 3D_MODEL."
                    : "When finished, TRELLIS.2 returns a geometry-only GLB with asset_type 3D_MODEL.");
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "edit_tencent_hunyuan_texture")]
    [Description("Submit a Tencent Hunyuan texture-edit request to 3D AI Studio.")]
    public async Task<TaskSubmissionResult> EditTencentHunyuanTexture(
        [Description("Public URL to the FBX model that should be textured.")] string fileUrl,
        [Description("Text prompt describing the desired texture. Use this or image/imageFilePath.")] string? prompt = null,
        [Description("Reference image as a data URI or base64 string. Use this or imageFilePath instead of prompt.")] string? image = null,
        [Description("Optional local file path to a reference image. The server converts it to a data URI automatically.")] string? imageFilePath = null,
        [Description("Enable PBR texture output. Only supported for prompt-based texturing.")] bool enablePbr = false)
    {
        try
        {
            var normalizedFileUrl = NormalizeRequiredValue(fileUrl, nameof(fileUrl));
            var normalizedPrompt = NormalizeOptionalValue(prompt);
            var resolvedImage = ImageInputResolver.ResolveOptionalImage(image, imageFilePath, nameof(image));

            ValidateTextureEditRequest(normalizedFileUrl, normalizedPrompt, resolvedImage, enablePbr);

            var response = await apiClient.SubmitTaskAsync(
                "v1/3d-models/tencent/texture-edit/",
                new TencentTextureEditApiRequest
                {
                    FileUrl = normalizedFileUrl,
                    Prompt = normalizedPrompt,
                    Image = resolvedImage,
                    EnablePbr = enablePbr
                });

            return new TaskSubmissionResult(
                Operation: "Tencent Hunyuan Texture Edit",
                TaskId: response.TaskId,
                CreatedAt: response.CreatedAt,
                StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
                OutputFormatHint: "When finished, texturing returns a textured 3D model with asset_type 3D_MODEL.");
        }
        catch (Exception exception) when (exception is ArgumentException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "remesh_tencent_hunyuan_model")]
    [Description("Submit a Tencent Hunyuan smart-topology remeshing request to 3D AI Studio.")]
    public async Task<TaskSubmissionResult> RemeshTencentHunyuanModel(
        [Description("Public URL to the source model file.")] string fileUrl,
        [Description("Input file type: GLB or OBJ.")] string fileType = "GLB",
        [Description("Optional target polygon type: triangle or quadrilateral.")] string? polygonType = null,
        [Description("Optional density preset: high, medium, or low.")] string? faceLevel = null)
    {
        try
        {
            var normalizedFileUrl = NormalizeRequiredValue(fileUrl, nameof(fileUrl));
            var normalizedFileType = NormalizeRequiredValue(fileType, nameof(fileType)).ToUpperInvariant();
            var normalizedPolygonType = NormalizeOptionalValue(polygonType)?.ToLowerInvariant();
            var normalizedFaceLevel = NormalizeOptionalValue(faceLevel)?.ToLowerInvariant();

            ValidateTopologyRequest(normalizedFileUrl, normalizedFileType, normalizedPolygonType, normalizedFaceLevel);

            var response = await apiClient.SubmitTaskAsync(
                "v1/3d-models/tencent/topology/",
                new TencentTopologyApiRequest
                {
                    FileUrl = normalizedFileUrl,
                    FileType = normalizedFileType,
                    PolygonType = normalizedPolygonType,
                    FaceLevel = normalizedFaceLevel
                });

            return new TaskSubmissionResult(
                Operation: "Tencent Hunyuan Smart Topology",
                TaskId: response.TaskId,
                CreatedAt: response.CreatedAt,
                StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
                OutputFormatHint: "When finished, remeshing returns a 3D model with asset_type 3D_MODEL.");
        }
        catch (Exception exception) when (exception is ArgumentException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static void ValidateTrellisRequest(
        string? image,
        string? imageUrl,
        string resolution,
        int steps,
        bool textures,
        int? textureSize,
        int decimationTarget,
        int? seed)
    {
        var hasImage = !string.IsNullOrWhiteSpace(image);
        var hasImageUrl = !string.IsNullOrWhiteSpace(imageUrl);

        if (hasImage == hasImageUrl)
        {
            throw new ArgumentException("Provide exactly one of image/imageFilePath or imageUrl.");
        }

        if (!SupportedTrellisResolutions.Contains(resolution))
        {
            throw new ArgumentException("resolution must be one of 512, 1024, or 1536.");
        }

        if (steps is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(steps), "steps must be between 1 and 50.");
        }

        if (!textures && textureSize is not null)
        {
            throw new ArgumentException("textureSize can only be provided when textures is true.");
        }

        if (textureSize is not null && !SupportedTrellisTextureSizes.Contains(textureSize.Value))
        {
            throw new ArgumentException("textureSize must be 1024, 2048, or 4096.");
        }

        if (decimationTarget is < 1000 or > 16000000)
        {
            throw new ArgumentOutOfRangeException(nameof(decimationTarget), "decimationTarget must be between 1000 and 16000000.");
        }

        if (seed is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seed), "seed must be 0 or greater.");
        }
    }

    private static void ValidateTextureEditRequest(string fileUrl, string? prompt, string? image, bool enablePbr)
    {
        var hasPrompt = !string.IsNullOrWhiteSpace(prompt);
        var hasImage = !string.IsNullOrWhiteSpace(image);

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("fileUrl is required.");
        }

        if (hasPrompt == hasImage)
        {
            throw new ArgumentException("Provide exactly one of prompt or image/imageFilePath.");
        }

        if (prompt is { Length: > 1024 })
        {
            throw new ArgumentException("prompt must be 1024 characters or fewer.");
        }

        if (enablePbr && hasImage)
        {
            throw new ArgumentException("enablePbr is only supported for prompt-based texturing.");
        }
    }

    private static void ValidateTopologyRequest(string fileUrl, string fileType, string? polygonType, string? faceLevel)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("fileUrl is required.");
        }

        if (!SupportedRemeshFileTypes.Contains(fileType))
        {
            throw new ArgumentException("fileType must be GLB or OBJ.");
        }

        if (polygonType is not null && !SupportedPolygonTypes.Contains(polygonType))
        {
            throw new ArgumentException("polygonType must be triangle or quadrilateral when provided.");
        }

        if (faceLevel is not null && !SupportedFaceLevels.Contains(faceLevel))
        {
            throw new ArgumentException("faceLevel must be high, medium, or low when provided.");
        }
    }

    private static string NormalizeRequiredValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
