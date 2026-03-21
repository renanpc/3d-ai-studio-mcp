using System.ComponentModel;
using ModelContextProtocol.Server;
using ThreeDAiStudioMcp.Clients;
using ThreeDAiStudioMcp.Models.Account;
using ThreeDAiStudioMcp.Models.Api;
using ThreeDAiStudioMcp.Models.Hunyuan;
using ThreeDAiStudioMcp.Models.Tasks;
using ThreeDAiStudioMcp.Utilities;

namespace ThreeDAiStudioMcp.Tools;

internal sealed class TencentHunyuanTools(ThreeDAiStudioApiClient apiClient)
{
    private static readonly HashSet<string> SupportedModels = new(StringComparer.OrdinalIgnoreCase)
    {
        "3.0",
        "3.1"
    };

    private static readonly HashSet<string> SupportedGenerateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Normal",
        "LowPoly",
        "Geometry",
        "Sketch"
    };

    private static readonly HashSet<string> SupportedPolygonTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "triangle",
        "quadrilateral"
    };

    private static readonly HashSet<string> Model30Views = new(StringComparer.OrdinalIgnoreCase)
    {
        "front",
        "left",
        "right",
        "back"
    };

    private static readonly HashSet<string> Model31Views = new(StringComparer.OrdinalIgnoreCase)
    {
        "front",
        "left",
        "right",
        "back",
        "top",
        "bottom",
        "left_front",
        "right_front"
    };

    [McpServerTool(Name = "generate_tencent_hunyuan_pro")]
    [Description("Submit a Tencent Hunyuan Pro 3D generation request to 3D AI Studio. Supports prompt, single-image, and multi-view generation.")]
    public async Task<TaskSubmissionResult> GenerateTencentHunyuanPro(
        [Description("Model version. Supported values: 3.0 or 3.1.")] string model = "3.1",
        [Description("Text prompt for text-to-3D generation.")] string? prompt = null,
        [Description("Reference image as a data URI or base64 string. Use this or imageFilePath.")] string? image = null,
        [Description("Optional local file path to a reference image. The server converts it to a data URI automatically.")] string? imageFilePath = null,
        [Description("Enable PBR textures. Adds extra credits.")] bool enablePbr = false,
        [Description("Target polygon count from 40000 to 1500000.")] int faceCount = 500000,
        [Description("Generation mode: Normal, LowPoly, Geometry, or Sketch.")] string generateType = "Normal",
        [Description("Polygon type used only with LowPoly: triangle or quadrilateral.")] string polygonType = "triangle",
        [Description("Optional multi-view image inputs. A front view is always required when this is provided.")] IReadOnlyList<HunyuanMultiViewImageInput>? multiViewImages = null)
    {
        try
        {
            var normalizedModel = NormalizeRequiredValue(model, nameof(model));
            var normalizedGenerateType = NormalizeRequiredValue(generateType, nameof(generateType));
            var normalizedPolygonType = NormalizeRequiredValue(polygonType, nameof(polygonType));
            var resolvedImage = ImageInputResolver.ResolveOptionalImage(image, imageFilePath, nameof(image));
            var resolvedMultiViewImages = ImageInputResolver.ResolveOptionalMultiViewImages(multiViewImages);

            ValidateProRequest(
                normalizedModel,
                prompt,
                resolvedImage,
                faceCount,
                normalizedGenerateType,
                normalizedPolygonType,
                resolvedMultiViewImages);

            var response = await apiClient.SubmitHunyuanProAsync(
                new HunyuanProGenerationApiRequest
                {
                    Model = normalizedModel,
                    Prompt = NormalizeOptionalValue(prompt),
                    Image = resolvedImage,
                    EnablePbr = enablePbr,
                    FaceCount = faceCount,
                    GenerateType = normalizedGenerateType,
                    PolygonType = normalizedGenerateType.Equals("LowPoly", StringComparison.OrdinalIgnoreCase)
                        ? normalizedPolygonType
                        : null,
                    MultiViewImages = resolvedMultiViewImages
                });

            return new TaskSubmissionResult(
                Operation: "Tencent Hunyuan Pro",
                TaskId: response.TaskId,
                CreatedAt: response.CreatedAt,
                StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
                OutputFormatHint: "When finished, Pro returns a GLB asset with asset_type 3D_MODEL.");
        }
        catch (Exception exception) when (exception is ArgumentException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "generate_tencent_hunyuan_rapid")]
    [Description("Submit a Tencent Hunyuan Rapid 3D generation request to 3D AI Studio. Supports prompt and single-image generation.")]
    public async Task<TaskSubmissionResult> GenerateTencentHunyuanRapid(
        [Description("Text prompt for text-to-3D generation.")] string? prompt = null,
        [Description("Reference image as a data URI or base64 string. Use this or imageFilePath.")] string? image = null,
        [Description("Optional local file path to a reference image. The server converts it to a data URI automatically.")] string? imageFilePath = null,
        [Description("Enable PBR textures. Adds extra credits.")] bool enablePbr = false,
        [Description("Enable geometry optimization.")] bool enableGeometry = false)
    {
        try
        {
            var resolvedImage = ImageInputResolver.ResolveOptionalImage(image, imageFilePath, nameof(image));
            ValidateRapidRequest(prompt, resolvedImage);

            var response = await apiClient.SubmitHunyuanRapidAsync(
                new HunyuanRapidGenerationApiRequest
                {
                    Prompt = NormalizeOptionalValue(prompt),
                    Image = resolvedImage,
                    EnablePbr = enablePbr,
                    EnableGeometry = enableGeometry
                });

            return new TaskSubmissionResult(
                Operation: "Tencent Hunyuan Rapid",
                TaskId: response.TaskId,
                CreatedAt: response.CreatedAt,
                StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
                OutputFormatHint: "When finished, Rapid returns a ZIP asset with asset_type ARCHIVE.");
        }
        catch (Exception exception) when (exception is ArgumentException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "get_generation_status")]
    [Description("Get the current status for a 3D AI Studio generation task.")]
    public async Task<GenerationStatusResult> GetGenerationStatus(
        [Description("The task_id returned by a generation submission request.")] string taskId)
    {
        try
        {
            var normalizedTaskId = NormalizeRequiredValue(taskId, nameof(taskId));
            return await apiClient.GetGenerationStatusAsync(normalizedTaskId);
        }
        catch (Exception exception) when (exception is ArgumentException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "get_credit_balance")]
    [Description("Get the current 3D AI Studio credit balance for the authenticated account.")]
    public async Task<CreditBalanceResult> GetCreditBalance()
    {
        try
        {
            return await apiClient.GetCreditBalanceAsync();
        }
        catch (Exception exception) when (exception is ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static void ValidateRapidRequest(string? prompt, string? image)
    {
        if (string.IsNullOrWhiteSpace(prompt) && string.IsNullOrWhiteSpace(image))
        {
            throw new ArgumentException("Rapid generation requires either a prompt or an image.");
        }
    }

    private static void ValidateProRequest(
        string model,
        string? prompt,
        string? image,
        int faceCount,
        string generateType,
        string polygonType,
        IReadOnlyList<HunyuanMultiViewImageApiInput>? multiViewImages)
    {
        if (!SupportedModels.Contains(model))
        {
            throw new ArgumentException("model must be either 3.0 or 3.1.");
        }

        if (faceCount is < 40000 or > 1500000)
        {
            throw new ArgumentOutOfRangeException(nameof(faceCount), "faceCount must be between 40000 and 1500000.");
        }

        if (!SupportedGenerateTypes.Contains(generateType))
        {
            throw new ArgumentException("generateType must be one of Normal, LowPoly, Geometry, or Sketch.");
        }

        if (!SupportedPolygonTypes.Contains(polygonType))
        {
            throw new ArgumentException("polygonType must be triangle or quadrilateral.");
        }

        if (model.Equals("3.1", StringComparison.OrdinalIgnoreCase) &&
            (generateType.Equals("LowPoly", StringComparison.OrdinalIgnoreCase) ||
             generateType.Equals("Sketch", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Model 3.1 does not support the LowPoly or Sketch generateType values.");
        }

        if (multiViewImages is { Count: > 0 })
        {
            ValidateMultiViewImages(model, multiViewImages);
        }

        var hasPrompt = !string.IsNullOrWhiteSpace(prompt);
        var hasImage = !string.IsNullOrWhiteSpace(image);
        var hasMultiView = multiViewImages is { Count: > 0 };

        if (!hasPrompt && !hasImage && !hasMultiView)
        {
            throw new ArgumentException("Pro generation requires a prompt, an image, or multiViewImages.");
        }

        if (hasPrompt && hasImage && !generateType.Equals("Sketch", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Providing both prompt and image is only supported when generateType is Sketch.");
        }
    }

    private static void ValidateMultiViewImages(string model, IReadOnlyList<HunyuanMultiViewImageApiInput> multiViewImages)
    {
        var allowedViews = model.Equals("3.0", StringComparison.OrdinalIgnoreCase) ? Model30Views : Model31Views;
        var suppliedViews = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var image in multiViewImages)
        {
            if (!allowedViews.Contains(image.ViewType))
            {
                throw new ArgumentException(
                    $"View type '{image.ViewType}' is not supported for model {model}.");
            }

            if (!suppliedViews.Add(image.ViewType))
            {
                throw new ArgumentException($"Duplicate multi-view input detected for '{image.ViewType}'.");
            }
        }

        if (!suppliedViews.Contains("front"))
        {
            throw new ArgumentException("multiViewImages must include at least one front view.");
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
