using CodeWF.Utils;

namespace CodeWF.Data;

public partial class Seed
{
    private static async Task AddPostsAsync(BlogDbContext dbContext, string assetDir)
    {
        List<CategoryEntity> cats = GetCategories(assetDir).ToList();
        await dbContext.Category.AddRangeAsync(cats);

        (BlogPostSeedDto[]? BlogPosts, string[]? Tags) postAndTags = await GetPostAsync(assetDir, cats.ToList());

        List<TagEntity> tags = GetTags(postAndTags.Tags)!.ToList();
        await dbContext.Tag.AddRangeAsync(tags);

        IEnumerable<PostEntity> posts = GetPosts(postAndTags.BlogPosts!.ToList(), cats, tags);
        await dbContext.Post.AddRangeAsync(posts);
    }

    private static IEnumerable<CategoryEntity> GetCategories(string assetDir)
    {
        string filePath = Path.Combine(assetDir, "site", "category.json");
        List<CategoryEntity> dataList = ReadFromFile<CategoryEntity>(filePath).ToList();
        dataList.ForEach(item =>
        {
            item.Id = Guid.NewGuid();
            item.Note = item.DisplayName;
        });

        return dataList!;
    }

    private static IEnumerable<TagEntity>? GetTags(string[]? tagSeeds)
    {
        if (tagSeeds == null || !tagSeeds.Any())
        {
            return null;
        }

        int id = 1;

        return tagSeeds
            .Select(tagSeed => new TagEntity { Id = id++, DisplayName = tagSeed, NormalizedName = tagSeed }).ToList();
    }

    private static IEnumerable<PostEntity> GetPosts(List<BlogPostSeedDto> posts, List<CategoryEntity> cats,
        List<TagEntity> tags)
    {
        List<PostEntity> postList = new List<PostEntity>();
        foreach (BlogPostSeedDto post in posts)
        {
            PostEntity newPost = new PostEntity
            {
                Id = Guid.NewGuid(),
                Title = post.Title,
                Slug = post.Slug,
                Author = post.Author,
                PostContent = post.Content,
                ContentAbstract = post.Description,
                CommentEnabled = true,
                CreateTimeUtc = post.Date,
                IsFeedIncluded = true,
                LastModifiedUtc = post.LastModifyDate,
                IsPublished = !post.Draft,
                IsDeleted = false,
                IsOriginal = post.Copyright == CopyRightType.Original,
                IsOutdated = false,
                OriginLink = post.OriginalLink,
                HeroImageUrl = post.Cover,
                IsFeatured = post.Banner
            };
            newPost.PubDateUtc = newPost.LastModifiedUtc ?? newPost.CreateTimeUtc;
            var input = $"{newPost.Slug.ToLower()}#{newPost.PubDateUtc.GetValueOrDefault():yyyyMMdd}";
            var checkSum = Helper.ComputeCheckSum(input);
            newPost.HashCheckSum = checkSum;

            post.Categories?.ForEach(cat =>
            {
                CategoryEntity catEntity = cats.First(entity => entity.DisplayName == cat);
                newPost.PostCategory.Add(new PostCategoryEntity { PostId = newPost.Id, CategoryId = catEntity.Id });
            });
            post.Tags?.ForEach(tag =>
            {
                TagEntity tagEntity = tags.First(entity => entity.DisplayName == tag);
                newPost.Tags.Add(tagEntity);
            });
            postList.Add(newPost);
        }

        return postList;
    }

    private static IEnumerable<FriendLinkEntity> GetFriendLinks(string assetDir)
    {
        string filePath = Path.Combine(assetDir, "site", "FriendLink.json");
        List<FriendLinkEntity> dataList = ReadFromFile<FriendLinkEntity>(filePath).ToList();
        dataList.ForEach(item => item.Id = Guid.NewGuid());
        return dataList;
    }

    private static IEnumerable<T> ReadFromFile<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new Exception($"Please config {filePath}");
        }

        string fileContent = File.ReadAllText(filePath);
        List<T>? dataList = JsonSerializer.Deserialize<IEnumerable<T>>(fileContent)?.ToList();
        if (dataList?.Any() != true)
        {
            throw new Exception($"Please config {filePath}");
        }

        return dataList;
    }


    private static IEnumerable<PageEntity> GetPages(string assetDir)
    {
        return new List<PageEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "About",
                Slug = "about",
                MetaDescription = "An Empty About Page",
                HtmlContent =
                    ContentProcessor.MarkdownToContent(File.ReadAllText(Path.Combine(assetDir, "site", "about.md")),
                        ContentProcessor.MarkdownConvertType.Html, false),
                HideSidebar = true,
                IsPublished = true,
                CreateTimeUtc = DateTime.UtcNow,
                UpdateTimeUtc = DateTime.UtcNow
            }
        };
    }
}