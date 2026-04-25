using WebApp.Models;

namespace WebApp.Extensions;

public static class ConstantUtil
{
    public const string DefaultCategory = "default";
    public static string GetBbsCategoryUrl(string slug) => $"/bbs/cat/{slug}";
    public static string GetBbsAlbumUrl(string slug) => $"/bbs/album/{slug}";
    public static string GetBbsPostUrl(BlogPost post) => $"/bbs/post/{post.Date?.Year}/{post.Date?.Month}/{post.Slug}";
    public static string GetDocUrl(string? slug) => $"/doc/{slug}";
    public static string GetToolUrl(string? slug) => slug switch
    {
        "slugify-string" => "/SlugifyString",
        "timestamp" => "/Timestamp",
        "ico" => "/Icon",
        "nuoche" => "/NuoChe",
        "fuli" => "/FuLi",
        null or "" => "/tool",
        _ => $"/Tool/{slug}"
    };

    public static string GetPostGithubPath(string? githubRepository, BlogPost? post) =>
        $"{githubRepository}/blob/main/{post?.Date?.Year:D4}/{post?.Date?.Month:D2}/{post?.Slug}.md";
}
