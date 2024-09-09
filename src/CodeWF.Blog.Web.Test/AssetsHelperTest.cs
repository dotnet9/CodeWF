using CodeWF.Blog.Web.Client.Helpers;

namespace CodeWF.Blog.Web.Test;

[TestClass]
public class AssetsHelperTest
{
    [TestMethod]
    public async Task Read_Markdown_File_Success()
    {
        var markdownFile = "D:\\github\\owner\\Assets.Dotnet9\\2021\\06\\dotnet-classlibrary-vanara-an-easy-to-use-windows-api-encapsulation-library.md";

        var blogPost = await AssetsHelper.ReadBlogPostAsync(markdownFile);

        Assert.IsNotNull(blogPost);
    }

    [TestMethod]
    public async Task Read_AllBlogPost_Success()
    {
        var markdownFile = "D:\\github\\owner\\Assets.Dotnet9";

        var blogPosts = await AssetsHelper.ReadBlogPostsAsync(markdownFile);

        Assert.IsNotNull(blogPosts);
        Assert.IsTrue(blogPosts.Count > 0);
    }
}
