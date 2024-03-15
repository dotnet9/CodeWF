namespace CodeWF.Tools.Core.Test;

[TestClass]
public class ImageHelperUnitTest
{
    [TestMethod]
    public async Task Test_ConvertImageToIconAsync_SUCCESS()
    {
        string sourceImg = "D://1.jpg";
        Assert.IsTrue(File.Exists(sourceImg), "Source image file does not exist.");
        await ImageHelper.ToIconAsync(sourceImg);

        string outputDirectory = Path.GetDirectoryName(sourceImg)!;
        string sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceImg);
        IEnumerable<string> expectedIconFiles = Enum.GetValues<IconSizeKind>().Select(size =>
            Path.Combine(outputDirectory,
                $"{sourceFileNameWithoutExtension}_{size.GetDescription()}.ico"));
        foreach (string iconFile in expectedIconFiles)
        {
            Assert.IsTrue(File.Exists(iconFile),
                $"Expected icon file does not exist: {iconFile}");
        }
    }
}