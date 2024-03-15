namespace CodeWF.Tools.Core.Helpers;

public static class ImageHelper
{
    public static async Task ToIconAsync(string sourceImagePath, string? outputDirectory = default,
        IconSizeKind sizeKind = IconSizeKind.Size16X16 | IconSizeKind.Size24X24 | IconSizeKind.Size32X32 |
                                IconSizeKind.Size48X48 |
                                IconSizeKind.Size64X64 | IconSizeKind.Size96X96 | IconSizeKind.Size128X128 |
                                IconSizeKind.Size256X256)
    {
        if (!File.Exists(sourceImagePath))
        {
            throw new FileNotFoundException("Source image file not found.", sourceImagePath);
        }

        outputDirectory ??= Path.GetDirectoryName(sourceImagePath)!;
        string sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceImagePath);

        foreach (Enum enumValue in Enum.GetValues(typeof(IconSizeKind)))
        {
            if (!sizeKind.HasFlag(enumValue))
            {
                continue;
            }

            string sizeInfo = enumValue.GetDescription()!;
            int width = sizeInfo.Split('x')[0].ToInt();

            string resizeImagePath = Path.Combine(outputDirectory, $"{sourceFileNameWithoutExtension}_{sizeInfo}.png");
            string targetIconPath = Path.Combine(outputDirectory, $"{sourceFileNameWithoutExtension}_{sizeInfo}.ico");

            FileHelper.DeleteFileIfExist(resizeImagePath);
            FileHelper.DeleteFileIfExist(targetIconPath);

            await ResizeImageSaveAsAsync(sourceImagePath, resizeImagePath, MagickFormat.Png32, width, width);

            using MagickImage img = new MagickImage(resizeImagePath);
            await img.WriteAsync(targetIconPath, MagickFormat.Ico);

            File.Delete(resizeImagePath);
        }
    }

    public static async Task ResizeImageSaveAsAsync(string sourceImagePath, string targetImagePath,
        MagickFormat imageFormat,
        int width, int height)
    {
        using MagickImage image = new MagickImage(sourceImagePath);
        image.Strip(); // 移除所有配置文件和注释信息，减小文件大小（可选）
        image.Resize(width, height);
        image.BackgroundColor = MagickColors.Transparent;
        await image.WriteAsync(targetImagePath, imageFormat);
    }
}

[Flags]
public enum IconSizeKind
{
    [Description("16x16")] Size16X16 = 1 << 0,
    [Description("24x24")] Size24X24 = 1 << 1,
    [Description("32x32")] Size32X32 = 1 << 2,
    [Description("48x48")] Size48X48 = 1 << 3,
    [Description("64x64")] Size64X64 = 1 << 4,
    [Description("96x96")] Size96X96 = 1 << 5,
    [Description("128x128")] Size128X128 = 1 << 6,
    [Description("256x256")] Size256X256 = 1 << 7
}