using CodeWF.MetaWeblog;
using Post = CodeWF.MetaWeblog.Post;
using Tag = CodeWF.MetaWeblog.Tag;

namespace CodeWF.Web;

public class MetaWeblogService(
    IBlogConfig blogConfig,
    ILogger<MetaWeblogService> logger,
    IBlogImageStorage blogImageStorage,
    IFileNameGenerator fileNameGenerator,
    IMediator mediator)
    : IMetaWeblogProvider
{
    public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecute(() =>
        {
            UserInfo user = new UserInfo
            {
                email = blogConfig.GeneralSettings.OwnerEmail,
                firstname = blogConfig.GeneralSettings.OwnerName,
                lastname = string.Empty,
                nickname = string.Empty,
                url = blogConfig.GeneralSettings.CanonicalPrefix,
                userid = key
            };

            return Task.FromResult(user);
        });
    }

    public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecute(() =>
        {
            BlogInfo blog = new BlogInfo
            {
                blogid = blogConfig.GeneralSettings.SiteTitle,
                blogName = blogConfig.GeneralSettings.SiteTitle,
                url = "/"
            };

            return Task.FromResult(new[] { blog });
        });
    }

    public Task<Post> GetPostAsync(string postid, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(postid.Trim(), out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(postid));
            }

            Core.PostFeature.Post post = await mediator.Send(new GetPostByIdQuery(id));
            return ToMetaWeblogPost(post);
        });
    }

    public async Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
    {
        EnsureUser(username, password);
        await Task.CompletedTask;

        return TryExecute(() =>
        {
            if (numberOfPosts < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfPosts));
            }

            // TODO: Get recent posts
            return Array.Empty<Post>();
        });
    }

    public Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            Guid[] cids = await GetCatIds(post.categories);
            if (cids.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(post.categories));
            }

            PostEditModel req = new PostEditModel
            {
                Title = post.title,
                Slug = post.wp_slug ?? ToSlug(post.title),
                EditorContent = post.description,
                Tags = post.mt_keywords,
                SelectedCatIds = cids,
                LanguageCode = "en-us",
                IsPublished = publish,
                EnableComment = true,
                FeedIncluded = true,
                PublishDate = DateTime.UtcNow
            };

            PostEntity p = await mediator.Send(new CreatePostCommand(req));
            return p.Id.ToString();
        });
    }

    public Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(postid.Trim(), out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(postid));
            }

            await mediator.Send(new DeletePostCommand(id, publish));
            return true;
        });
    }

    public Task<bool> EditPostAsync(string postid, string username, string password, Post post, bool publish)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(postid.Trim(), out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(postid));
            }

            Guid[] cids = await GetCatIds(post.categories);
            if (cids.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(post.categories));
            }

            PostEditModel req = new PostEditModel
            {
                Title = post.title,
                Slug = post.wp_slug ?? ToSlug(post.title),
                EditorContent = post.description,
                Tags = post.mt_keywords,
                SelectedCatIds = cids,
                LanguageCode = "en-us",
                IsPublished = publish,
                EnableComment = true,
                FeedIncluded = true,
                PublishDate = DateTime.UtcNow
            };

            await mediator.Send(new UpdatePostCommand(id, req));
            return true;
        });
    }

    public Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            IReadOnlyList<Category> cats = await mediator.Send(new GetCategoriesQuery());
            CategoryInfo[] catInfos = cats.Select(p => new CategoryInfo
            {
                title = p.DisplayName,
                categoryid = p.Id.ToString(),
                description = p.Note,
                htmlUrl = $"/category/{p.RouteName}",
                rssUrl = $"/rss/{p.RouteName}"
            }).ToArray();

            return catInfos;
        });
    }

    public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            await mediator.Send(new CreateCategoryCommand
            {
                DisplayName = category.name.Trim(),
                RouteName = category.slug.ToLower(),
                Note = category.description.Trim()
            });

            return 996;
        });
    }

    public Task<Tag[]> GetTagsAsync(string blogid, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            IReadOnlyList<string> names = await mediator.Send(new GetTagNamesQuery());
            Tag[] tags = names.Select(p => new Tag { name = p }).ToArray();

            return tags;
        });
    }

    public Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password,
        MediaObject mediaObject)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            // TODO: Check extension names

            byte[] bits = Convert.FromBase64String(mediaObject.bits);

            string pFilename = fileNameGenerator.GetFileName(mediaObject.name);
            string filename = await blogImageStorage.InsertAsync(pFilename, bits);

            string imageUrl =
                $"{Helper.ResolveRootUrl(null, blogConfig.GeneralSettings.CanonicalPrefix, true)}image/{filename}";

            MediaObjectInfo objectInfo = new MediaObjectInfo { url = imageUrl };
            return objectInfo;
        });
    }

    public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(pageid, out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(pageid));
            }

            BlogPage page = await mediator.Send(new GetPageByIdQuery(id));
            return ToMetaWeblogPage(page);
        });
    }

    public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (numPages < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numPages));
            }

            IReadOnlyList<BlogPage> pages = await mediator.Send(new GetPagesQuery(numPages));
            IEnumerable<Page> mPages = pages.Select(ToMetaWeblogPage);

            return mPages.ToArray();
        });
    }

    public async Task<Author[]> GetAuthorsAsync(string blogid, string username, string password)
    {
        EnsureUser(username, password);
        await Task.CompletedTask;

        return TryExecute(() =>
        {
            return new[] { new Author { display_name = blogConfig.GeneralSettings.OwnerName } };
        });
    }

    public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            EditPageRequest pageRequest = new EditPageRequest
            {
                Title = page.title,
                HideSidebar = true,
                MetaDescription = string.Empty,
                RawHtmlContent = page.description,
                CssContent = string.Empty,
                IsPublished = publish,
                Slug = ToSlug(page.title)
            };

            Guid uid = await mediator.Send(new CreatePageCommand(pageRequest));
            return uid.ToString();
        });
    }

    public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page,
        bool publish)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(pageid, out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(pageid));
            }

            EditPageRequest pageRequest = new EditPageRequest
            {
                Title = page.title,
                HideSidebar = true,
                MetaDescription = string.Empty,
                RawHtmlContent = page.description,
                CssContent = string.Empty,
                IsPublished = publish,
                Slug = ToSlug(page.title)
            };

            await mediator.Send(new UpdatePageCommand(id, pageRequest));
            return true;
        });
    }

    public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
    {
        EnsureUser(username, password);

        return TryExecuteAsync(async () =>
        {
            if (!Guid.TryParse(pageid, out Guid id))
            {
                throw new ArgumentException("Invalid ID", nameof(pageid));
            }

            await mediator.Send(new DeletePageCommand(id));
            return true;
        });
    }

    private void EnsureUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        string pwdHash = Helper.HashPassword(password.Trim());

        if (string.Compare(username.Trim(), "codewf", StringComparison.Ordinal) == 0 &&
            string.Compare(pwdHash, blogConfig.AdvancedSettings.MetaWeblogPasswordHash.Trim(),
                StringComparison.Ordinal) == 0)
        {
            return;
        }

        throw new MetaWeblogException("Authentication failed.");
    }

    private string ToSlug(string title)
    {
        string engSlug = title.GenerateSlug();
        if (!string.IsNullOrWhiteSpace(engSlug))
        {
            return engSlug;
        }

        // Chinese and other language title
        byte[] bytes = Encoding.Unicode.GetBytes(title);
        IEnumerable<string> hexArray = bytes.Select(b => $"{b:x2}");
        string hexName = string.Join(string.Empty, hexArray);

        return hexName;
    }

    private Post ToMetaWeblogPost(Core.PostFeature.Post post)
    {
        if (!post.IsPublished)
        {
            return null;
        }

        DateTime pubDate = post.PubDateUtc.GetValueOrDefault();
        string link = $"/post/{pubDate.Year}/{pubDate.Month}/{pubDate.Day}/{post.Slug.Trim().ToLower()}";

        Post mPost = new Post
        {
            postid = post.Id,
            categories = post.Categories.Select(p => p.DisplayName).ToArray(),
            dateCreated = post.CreateTimeUtc,
            description = post.ContentAbstract,
            link = link,
            permalink = $"{Helper.ResolveRootUrl(null, blogConfig.GeneralSettings.CanonicalPrefix, true)}/{link}",
            title = post.Title,
            wp_slug = post.Slug,
            mt_keywords = string.Join(',', post.Tags.Select(p => p.DisplayName)),
            mt_excerpt = post.ContentAbstract,
            userid = blogConfig.GeneralSettings.OwnerName
        };

        return mPost;
    }

    private Page ToMetaWeblogPage(BlogPage blogPage)
    {
        Page mPage = new Page
        {
            title = blogPage.Title,
            description = blogPage.RawHtmlContent,
            dateCreated = blogPage.CreateTimeUtc,
            categories = Array.Empty<string>(),
            page_id = blogPage.Id.ToString(),
            wp_author_id = blogConfig.GeneralSettings.OwnerName
        };

        return mPage;
    }

    private async Task<Guid[]> GetCatIds(string[] mPostCategories)
    {
        IReadOnlyList<Category> allCats = await mediator.Send(new GetCategoriesQuery());
        Guid[] cids = (from postCategory in mPostCategories
            select allCats.FirstOrDefault(category => category.DisplayName == postCategory)
            into cat
            where null != cat
            select cat.Id).ToArray();

        return cids;
    }

    private T TryExecute<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw new MetaWeblogException(e.Message);
        }
    }

    private async Task<T> TryExecuteAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw new MetaWeblogException(e.Message);
        }
    }
}