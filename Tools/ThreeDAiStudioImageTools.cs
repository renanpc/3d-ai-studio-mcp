using System.ComponentModel;
using ModelContextProtocol.Server;
using ThreeDAiStudioMcp.Clients;
using ThreeDAiStudioMcp.Models.Api;
using ThreeDAiStudioMcp.Models.Images;
using ThreeDAiStudioMcp.Models.Tasks;
using ThreeDAiStudioMcp.Utilities;

namespace ThreeDAiStudioMcp.Tools;

internal sealed class ThreeDAiStudioImageTools(ThreeDAiStudioApiClient apiClient)
{
    private static readonly HashSet<string> SupportedOutputFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "png",
        "jpeg",
        "webp"
    };

    private static readonly HashSet<string> GeminiAspectRatios = new(StringComparer.OrdinalIgnoreCase)
    {
        "auto",
        "21:9",
        "16:9",
        "3:2",
        "4:3",
        "5:4",
        "1:1",
        "4:5",
        "3:4",
        "2:3",
        "9:16"
    };

    private static readonly HashSet<string> Gemini31AspectRatios = new(StringComparer.OrdinalIgnoreCase)
    {
        "auto",
        "21:9",
        "16:9",
        "3:2",
        "4:3",
        "5:4",
        "1:1",
        "4:5",
        "3:4",
        "2:3",
        "9:16",
        "1:4",
        "4:1",
        "1:8",
        "8:1"
    };

    private static readonly HashSet<string> Gemini3ProResolutions = new(StringComparer.OrdinalIgnoreCase)
    {
        "1K",
        "2K",
        "4K"
    };

    private static readonly HashSet<string> Gemini31Resolutions = new(StringComparer.OrdinalIgnoreCase)
    {
        "512px",
        "1K",
        "2K",
        "4K"
    };

    private static readonly HashSet<string> SeedreamImageSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        "square_hd",
        "square",
        "portrait_4_3",
        "portrait_16_9",
        "landscape_4_3",
        "landscape_16_9",
        "auto_2K",
        "auto_3K"
    };

    [McpServerTool(Name = "generate_gemini3_pro_image")]
    [Description("Generate images with 3D AI Studio's Gemini 3 Pro model.")]
    public Task<TaskSubmissionResult> GenerateGemini3ProImage(
        [Description("Text prompt for image generation.")] string prompt,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio.")] string aspectRatio = "auto",
        [Description("Resolution: 1K, 2K, or 4K.")] string resolution = "1K",
        [Description("Number of images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiGenerateAsync(
            "Gemini 3 Pro",
            "v1/images/gemini/3/pro/generate/",
            prompt,
            outputFormat,
            aspectRatio,
            resolution,
            numImages,
            GeminiAspectRatios,
            Gemini3ProResolutions);

    [McpServerTool(Name = "edit_gemini3_pro_image")]
    [Description("Edit images with 3D AI Studio's Gemini 3 Pro model.")]
    public Task<TaskSubmissionResult> EditGemini3ProImage(
        [Description("Text instruction describing the edit.")] string prompt,
        [Description("Base64/data URI input images. Can be combined with imageFilePaths.")] IReadOnlyList<string>? images = null,
        [Description("Optional local file paths to input images. The server converts them to data URIs automatically.")] IReadOnlyList<string>? imageFilePaths = null,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio.")] string aspectRatio = "auto",
        [Description("Resolution: 1K, 2K, or 4K.")] string resolution = "1K",
        [Description("Number of output images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiEditAsync(
            "Gemini 3 Pro",
            "v1/images/gemini/3/pro/edit/",
            prompt,
            images,
            imageFilePaths,
            outputFormat,
            aspectRatio,
            resolution,
            numImages,
            GeminiAspectRatios,
            Gemini3ProResolutions);

    [McpServerTool(Name = "generate_gemini31_flash_image")]
    [Description("Generate images with 3D AI Studio's Gemini 3.1 Flash model.")]
    public Task<TaskSubmissionResult> GenerateGemini31FlashImage(
        [Description("Text prompt for image generation.")] string prompt,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio, including extreme formats like 1:8 and 8:1.")] string aspectRatio = "auto",
        [Description("Resolution: 512px, 1K, 2K, or 4K.")] string resolution = "1K",
        [Description("Number of images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiGenerateAsync(
            "Gemini 3.1 Flash",
            "v1/images/gemini/3.1/flash/generate/",
            prompt,
            outputFormat,
            aspectRatio,
            resolution,
            numImages,
            Gemini31AspectRatios,
            Gemini31Resolutions);

    [McpServerTool(Name = "edit_gemini31_flash_image")]
    [Description("Edit images with 3D AI Studio's Gemini 3.1 Flash model.")]
    public Task<TaskSubmissionResult> EditGemini31FlashImage(
        [Description("Text instruction describing the edit.")] string prompt,
        [Description("Base64/data URI input images. Can be combined with imageFilePaths.")] IReadOnlyList<string>? images = null,
        [Description("Optional local file paths to input images. The server converts them to data URIs automatically.")] IReadOnlyList<string>? imageFilePaths = null,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio, including extreme formats like 1:8 and 8:1.")] string aspectRatio = "auto",
        [Description("Resolution: 512px, 1K, 2K, or 4K.")] string resolution = "1K",
        [Description("Number of output images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiEditAsync(
            "Gemini 3.1 Flash",
            "v1/images/gemini/3.1/flash/edit/",
            prompt,
            images,
            imageFilePaths,
            outputFormat,
            aspectRatio,
            resolution,
            numImages,
            Gemini31AspectRatios,
            Gemini31Resolutions);

    [McpServerTool(Name = "generate_gemini25_flash_image")]
    [Description("Generate images with 3D AI Studio's Gemini 2.5 Flash model.")]
    public Task<TaskSubmissionResult> GenerateGemini25FlashImage(
        [Description("Text prompt for image generation.")] string prompt,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio.")] string aspectRatio = "auto",
        [Description("Number of images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiGenerateAsync(
            "Gemini 2.5 Flash",
            "v1/images/gemini/2.5/flash/generate/",
            prompt,
            outputFormat,
            aspectRatio,
            null,
            numImages,
            GeminiAspectRatios,
            null);

    [McpServerTool(Name = "edit_gemini25_flash_image")]
    [Description("Edit images with 3D AI Studio's Gemini 2.5 Flash model.")]
    public Task<TaskSubmissionResult> EditGemini25FlashImage(
        [Description("Text instruction describing the edit.")] string prompt,
        [Description("Base64/data URI input images. Can be combined with imageFilePaths.")] IReadOnlyList<string>? images = null,
        [Description("Optional local file paths to input images. The server converts them to data URIs automatically.")] IReadOnlyList<string>? imageFilePaths = null,
        [Description("Output format: png, jpeg, or webp.")] string outputFormat = "png",
        [Description("Aspect ratio.")] string aspectRatio = "auto",
        [Description("Number of output images from 1 to 4.")] int numImages = 1) =>
        SubmitGeminiEditAsync(
            "Gemini 2.5 Flash",
            "v1/images/gemini/2.5/flash/edit/",
            prompt,
            images,
            imageFilePaths,
            outputFormat,
            aspectRatio,
            null,
            numImages,
            GeminiAspectRatios,
            null);

    [McpServerTool(Name = "generate_seedream_v5_lite_image")]
    [Description("Generate images with 3D AI Studio's Seedream V5 Lite model.")]
    public async Task<TaskSubmissionResult> GenerateSeedreamV5LiteImage(
        [Description("Text prompt for image generation.")] string prompt,
        [Description("Image size preset.")] string imageSize = "auto_2K",
        [Description("Number of images from 1 to 6.")] int numImages = 1,
        [Description("Optional seed for reproducible output.")] int? seed = null,
        [Description("Enable content safety filtering.")] bool enableSafetyChecker = true)
    {
        try
        {
            var normalizedPrompt = NormalizeRequiredValue(prompt, nameof(prompt));
            var normalizedImageSize = NormalizeRequiredValue(imageSize, nameof(imageSize));

            ValidateSeedreamRequest(normalizedPrompt, normalizedImageSize, numImages, seed);

            var response = await apiClient.SubmitTaskAsync(
                "v1/images/seedream/v5/lite/generate/",
                new SeedreamImageGenerationApiRequest
                {
                    Prompt = normalizedPrompt,
                    ImageSize = normalizedImageSize,
                    NumImages = numImages,
                    Seed = seed,
                    EnableSafetyChecker = enableSafetyChecker
                });

            return CreateImageSubmissionResult("Seedream V5 Lite", response);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    [McpServerTool(Name = "edit_seedream_v5_lite_image")]
    [Description("Edit images with 3D AI Studio's Seedream V5 Lite model.")]
    public async Task<TaskSubmissionResult> EditSeedreamV5LiteImage(
        [Description("Text instruction describing the edit.")] string prompt,
        [Description("Base64/data URI input images. Can be combined with imageFilePaths.")] IReadOnlyList<string>? images = null,
        [Description("Optional local file paths to input images. The server converts them to data URIs automatically.")] IReadOnlyList<string>? imageFilePaths = null,
        [Description("Image size preset.")] string imageSize = "auto_2K",
        [Description("Number of output images from 1 to 6.")] int numImages = 1,
        [Description("Optional seed for reproducible output.")] int? seed = null,
        [Description("Enable content safety filtering.")] bool enableSafetyChecker = true)
    {
        try
        {
            var normalizedPrompt = NormalizeRequiredValue(prompt, nameof(prompt));
            var normalizedImageSize = NormalizeRequiredValue(imageSize, nameof(imageSize));
            var resolvedImages = ImageInputResolver.ResolveRequiredImages(images, imageFilePaths, nameof(images), maxCount: 10);

            ValidateSeedreamRequest(normalizedPrompt, normalizedImageSize, numImages, seed);

            var response = await apiClient.SubmitTaskAsync(
                "v1/images/seedream/v5/lite/edit/",
                new SeedreamImageEditApiRequest
                {
                    Prompt = normalizedPrompt,
                    ImageUrls = resolvedImages,
                    ImageSize = normalizedImageSize,
                    NumImages = numImages,
                    Seed = seed,
                    EnableSafetyChecker = enableSafetyChecker
                });

            return CreateImageSubmissionResult("Seedream V5 Lite", response);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private async Task<TaskSubmissionResult> SubmitGeminiGenerateAsync(
        string modelName,
        string relativePath,
        string prompt,
        string outputFormat,
        string aspectRatio,
        string? resolution,
        int numImages,
        HashSet<string> allowedAspectRatios,
        HashSet<string>? allowedResolutions)
    {
        try
        {
            var normalizedPrompt = NormalizeRequiredValue(prompt, nameof(prompt));
            var normalizedOutputFormat = NormalizeRequiredValue(outputFormat, nameof(outputFormat)).ToLowerInvariant();
            var normalizedAspectRatio = NormalizeRequiredValue(aspectRatio, nameof(aspectRatio));
            var normalizedResolution = NormalizeOptionalValue(resolution);

            ValidateGeminiRequest(
                normalizedPrompt,
                normalizedOutputFormat,
                normalizedAspectRatio,
                normalizedResolution,
                numImages,
                allowedAspectRatios,
                allowedResolutions);

            var response = await apiClient.SubmitTaskAsync(
                relativePath,
                new GeminiImageGenerationApiRequest
                {
                    Prompt = normalizedPrompt,
                    OutputFormat = normalizedOutputFormat,
                    AspectRatio = normalizedAspectRatio,
                    Resolution = normalizedResolution,
                    NumImages = numImages
                });

            return CreateImageSubmissionResult(modelName, response);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private async Task<TaskSubmissionResult> SubmitGeminiEditAsync(
        string modelName,
        string relativePath,
        string prompt,
        IReadOnlyList<string>? images,
        IReadOnlyList<string>? imageFilePaths,
        string outputFormat,
        string aspectRatio,
        string? resolution,
        int numImages,
        HashSet<string> allowedAspectRatios,
        HashSet<string>? allowedResolutions)
    {
        try
        {
            var normalizedPrompt = NormalizeRequiredValue(prompt, nameof(prompt));
            var normalizedOutputFormat = NormalizeRequiredValue(outputFormat, nameof(outputFormat)).ToLowerInvariant();
            var normalizedAspectRatio = NormalizeRequiredValue(aspectRatio, nameof(aspectRatio));
            var normalizedResolution = NormalizeOptionalValue(resolution);
            var resolvedImages = ImageInputResolver.ResolveRequiredImages(images, imageFilePaths, nameof(images), maxCount: 14);

            ValidateGeminiRequest(
                normalizedPrompt,
                normalizedOutputFormat,
                normalizedAspectRatio,
                normalizedResolution,
                numImages,
                allowedAspectRatios,
                allowedResolutions);

            var response = await apiClient.SubmitTaskAsync(
                relativePath,
                new GeminiImageEditApiRequest
                {
                    Prompt = normalizedPrompt,
                    Images = resolvedImages,
                    OutputFormat = normalizedOutputFormat,
                    AspectRatio = normalizedAspectRatio,
                    Resolution = normalizedResolution,
                    NumImages = numImages
                });

            return CreateImageSubmissionResult(modelName, response);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or FileNotFoundException or ThreeDAiStudioApiException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static void ValidateGeminiRequest(
        string prompt,
        string outputFormat,
        string aspectRatio,
        string? resolution,
        int numImages,
        HashSet<string> allowedAspectRatios,
        HashSet<string>? allowedResolutions)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("prompt is required.");
        }

        if (!SupportedOutputFormats.Contains(outputFormat))
        {
            throw new ArgumentException("outputFormat must be png, jpeg, or webp.");
        }

        if (!allowedAspectRatios.Contains(aspectRatio))
        {
            throw new ArgumentException("aspectRatio is not supported for this Gemini model.");
        }

        if (allowedResolutions is null)
        {
            if (resolution is not null)
            {
                throw new ArgumentException("This Gemini model does not support a resolution parameter.");
            }
        }
        else if (resolution is null || !allowedResolutions.Contains(resolution))
        {
            throw new ArgumentException("resolution is not supported for this Gemini model.");
        }

        if (numImages is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(numImages), "numImages must be between 1 and 4.");
        }
    }

    private static void ValidateSeedreamRequest(string prompt, string imageSize, int numImages, int? seed)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("prompt is required.");
        }

        if (!SeedreamImageSizes.Contains(imageSize))
        {
            throw new ArgumentException("imageSize is not supported for Seedream V5 Lite.");
        }

        if (numImages is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(numImages), "numImages must be between 1 and 6.");
        }

        if (seed is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seed), "seed must be 0 or greater.");
        }
    }

    private static TaskSubmissionResult CreateImageSubmissionResult(string modelName, TaskSubmissionResponse response) =>
        new(
            Operation: modelName,
            TaskId: response.TaskId,
            CreatedAt: response.CreatedAt,
            StatusEndpoint: $"/v1/generation-request/{response.TaskId}/status/",
            OutputFormatHint: "When finished, image generation returns one or more IMAGE assets.");

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
