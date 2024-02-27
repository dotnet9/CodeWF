using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodeWF.Web.PagedList;

public sealed class TagBuilderFactory
{
    public TagBuilder Create(string tagName)
    {
        return new TagBuilder(tagName);
    }
}