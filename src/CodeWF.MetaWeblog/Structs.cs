// ReSharper disable InconsistentNaming

namespace CodeWF.MetaWeblog;

public class BlogInfo
{
    public string blogid;
    public string blogName;
    public string url;
}

public class CategoryInfo
{
    public string categoryid;
    public string description;
    public string htmlUrl;
    public string rssUrl;
    public string title;
}

public class NewCategory
{
    public string description;
    public string name;
    public int parent_id;
    public string slug;
}

public class Tag
{
    public string name;
}

public class Enclosure
{
    public int length;
    public string type;
    public string url;
}

public class Post
{
    public string[] categories;
    public DateTime dateCreated;
    public string description;
    public string link;
    public int mt_allow_comments;
    public string mt_basename;
    public string mt_excerpt;
    public string mt_keywords;
    public string permalink;
    public object postid;
    public string title;
    public string userid;
    public string wp_post_thumbnail;
    public string wp_slug;
}

public class Source
{
    public string name;
    public string url;
}

public class UserInfo
{
    public string email;
    public string firstname;
    public string lastname;
    public string nickname;
    public string url;
    public string userid;
}

public class MediaObject
{
    public string bits;
    public string name;
    public string type;
}

public class MediaObjectInfo
{
    public string url;
}

public class Page
{
    public string[] categories;
    public DateTime dateCreated;
    public string description;
    public string page_id;
    public string page_parent_id;
    public string title;
    public string wp_author_id;
}

public class Author
{
    public string display_name;
    public string meta_value;
    public string user_id;
    public string user_login;
}