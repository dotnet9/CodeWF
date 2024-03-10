namespace CodeWF.Data;

public partial class Seed
{
    private static async Task AddPostsAsync(BlogDbContext dbContext)
    {
        await dbContext.Category.AddRangeAsync(GetCategories());
        await dbContext.Tag.AddRangeAsync(GetTags());
        IEnumerable<PostEntity> posts = GetPosts();
        posts.ForEach(post =>
        {
            post.Tags = dbContext.Tag.ToList();
            post.PostCategory = dbContext.PostCategory.ToList();
        });
    }

    private static IEnumerable<TagEntity> GetTags()
    {
        return new List<TagEntity>
        {
            new() { DisplayName = AppInfo.AppInfo.BlogName, NormalizedName = "codewf" },
            new() { DisplayName = ".NET", NormalizedName = "dot-net" }
        };
    }

    private static IEnumerable<CategoryEntity> GetCategories()
    {
        return new List<CategoryEntity>
        {
            new()
            {
                Id = Guid.Parse("b0c15707-dfc8-4b09-9aa0-5bfca744c50b"),
                DisplayName = "Default",
                Note = "Default Category",
                RouteName = "default"
            }
        };
    }

    private static IEnumerable<PostEntity> GetPosts()
    {
        // Add example post
        string content = "CodeWF is the blog system for https://codewf.com. Powered by .NET 9.";

        PostEntity post = new PostEntity
        {
            Id = Guid.NewGuid(),
            Title = "Welcome to CodeWF",
            Slug = "welcome-to-codewf",
            Author = "admin",
            PostContent = content,
            CommentEnabled = true,
            CreateTimeUtc = DateTime.UtcNow,
            ContentAbstract = content,
            IsPublished = true,
            IsFeatured = true,
            IsFeedIncluded = true,
            LastModifiedUtc = DateTime.UtcNow,
            PubDateUtc = DateTime.UtcNow,
            ContentLanguageCode = "en-us",
            HashCheckSum = -1688639577,
            IsOriginal = true
        };

        return new List<PostEntity> { post };
    }

    private static IEnumerable<FriendLinkEntity> GetFriendLinks()
    {
        return new List<FriendLinkEntity>
        {
            new() { Id = Guid.NewGuid(), Title = AppInfo.AppInfo.BlogName, LinkUrl = DomainType.CodeWFCom.Description() }
        };
    }

    private static IEnumerable<PageEntity> GetPages()
    {
        return new List<PageEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "About",
                Slug = "about",
                MetaDescription = "An Empty About Page",
                HtmlContent = "<h3>An Empty About Page</h3>",
                HideSidebar = true,
                IsPublished = true,
                CreateTimeUtc = DateTime.UtcNow,
                UpdateTimeUtc = DateTime.UtcNow
            }
        };
    }
}