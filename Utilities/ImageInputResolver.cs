using ThreeDAiStudioMcp.Models.Hunyuan;

namespace ThreeDAiStudioMcp.Utilities;

internal static class ImageInputResolver
{
    private static readonly IReadOnlyDictionary<string, string> MimeTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".bmp"] = "image/bmp"
        };

    public static string? ResolveOptionalImage(string? image, string? imageFilePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(image) && string.IsNullOrWhiteSpace(imageFilePath))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(image) && !string.IsNullOrWhiteSpace(imageFilePath))
        {
            throw new ArgumentException(
                $"Provide either {parameterName} or {parameterName}FilePath, but not both.");
        }

        if (!string.IsNullOrWhiteSpace(image))
        {
            return image.Trim();
        }

        return CreateDataUriFromFile(imageFilePath!);
    }

    public static IReadOnlyList<HunyuanMultiViewImageApiInput>? ResolveOptionalMultiViewImages(
        IReadOnlyList<HunyuanMultiViewImageInput>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
        {
            return null;
        }

        return inputs.Select(
                input => new HunyuanMultiViewImageApiInput
                {
                    ViewType = NormalizeViewType(input.ViewType),
                    ViewImage = ResolveOptionalImage(input.ViewImage, input.ViewImageFilePath, $"{NormalizeViewType(input.ViewType)}ViewImage")
                        ?? throw new ArgumentException($"Multi-view input '{NormalizeViewType(input.ViewType)}' requires a viewImage or viewImageFilePath.")
                })
            .ToArray();
    }

    public static IReadOnlyList<string> ResolveRequiredImages(
        IReadOnlyList<string>? images,
        IReadOnlyList<string>? imageFilePaths,
        string parameterName,
        int maxCount)
    {
        var resolvedImages = ResolveImages(images, imageFilePaths, parameterName);
        if (resolvedImages.Count == 0)
        {
            throw new ArgumentException(
                $"Provide at least one {parameterName} entry or {parameterName}FilePaths entry.");
        }

        if (resolvedImages.Count > maxCount)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"A maximum of {maxCount} images is supported.");
        }

        return resolvedImages;
    }

    private static List<string> ResolveImages(
        IReadOnlyList<string>? images,
        IReadOnlyList<string>? imageFilePaths,
        string parameterName)
    {
        var resolvedImages = new List<string>();

        if (images is not null)
        {
            foreach (var image in images)
            {
                if (string.IsNullOrWhiteSpace(image))
                {
                    throw new ArgumentException($"Each {parameterName} entry must be non-empty.");
                }

                resolvedImages.Add(image.Trim());
            }
        }

        if (imageFilePaths is not null)
        {
            foreach (var imageFilePath in imageFilePaths)
            {
                if (string.IsNullOrWhiteSpace(imageFilePath))
                {
                    throw new ArgumentException($"Each {parameterName}FilePaths entry must be non-empty.");
                }

                resolvedImages.Add(CreateDataUriFromFile(imageFilePath));
            }
        }

        return resolvedImages;
    }

    private static string NormalizeViewType(string? viewType)
    {
        if (string.IsNullOrWhiteSpace(viewType))
        {
            throw new ArgumentException("Each multiViewImages entry must include a non-empty viewType.");
        }

        return viewType.Trim();
    }

    private static string CreateDataUriFromFile(string imageFilePath)
    {
        var fullPath = Path.GetFullPath(imageFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Image file was not found: {fullPath}", fullPath);
        }

        var extension = Path.GetExtension(fullPath);
        if (!MimeTypes.TryGetValue(extension, out var mimeType))
        {
            mimeType = "application/octet-stream";
        }

        var bytes = File.ReadAllBytes(fullPath);
        return $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
    }
}
