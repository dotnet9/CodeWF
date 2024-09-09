using CodeWF.Blog.Web.Client.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeWF.Blog.Web.Client.Helpers;

public static class AssetsHelper
{
    public static async Task<List<BlogPost>> ReadBlogPostsAsync(string assetsDir)
    {
        var posts = new List<BlogPost>();
        const int startYear = 2019;
        var endYear = DateTime.Now.Year;

        for (var start = startYear; start <= endYear; start++)
        {
            var postDir = Path.Combine(assetsDir, start.ToString());
            var postFiles = System.IO.Directory.GetFiles(postDir, "*.md", SearchOption.AllDirectories);
            foreach (var postFile in postFiles)
            {
                var blogPost = await AssetsHelper.ReadBlogPostAsync(postFile);
                posts.Add(blogPost);
            }
        }

        return posts;
    }

    public static async Task<BlogPost> ReadBlogPostAsync(string markdownFilePath)
    {
        var markdown = await File.ReadAllTextAsync(markdownFilePath);
        var endOfFrontMatter = markdown.IndexOf("---", 3, StringComparison.Ordinal);
        if (endOfFrontMatter == -1)
        {
            throw new InvalidOperationException("Invalid markdown format. No ending '---' found for Front Matter.");
        }

        var frontMatterText = markdown[..(endOfFrontMatter + 3)];
        var markdownContent = markdown[(endOfFrontMatter + 3)..].Trim();
        if (frontMatterText.StartsWith("---"))
        {
            frontMatterText = frontMatterText[3..].Trim();
        }

        if (frontMatterText.EndsWith("---"))
        {
            frontMatterText = frontMatterText[..^3].Trim();
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        BlogPost blogPost;
        try
        {
            blogPost = deserializer.Deserialize<BlogPost>(frontMatterText);
        }
        catch (Exception ex)
        {
            string a = ex.Message;

            blogPost = new BlogPost();
        }

        blogPost.Content = markdownContent;

        return blogPost;
    }
}