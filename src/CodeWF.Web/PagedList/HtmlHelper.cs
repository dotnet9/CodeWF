using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Immutable;

namespace CodeWF.Web.PagedList;

public class HtmlHelper(TagBuilderFactory tagBuilderFactory)
{
    public string PagedListPager(IPagedList pagedList, Func<int, string> generatePageUrl,
        PagedListRenderOptions options)
    {
        IPagedList list = pagedList ?? new BasePagedList<int>(ImmutableList<int>.Empty, 1, 10, 0);

        if (list.PageCount <= 1)
        {
            return null;
        }

        List<TagBuilder> listItemLinks = new List<TagBuilder>();

        //calculate start and end of range of page numbers
        int firstPageToDisplay = 1;
        int lastPageToDisplay = list.PageCount;
        int pageNumbersToDisplay = lastPageToDisplay;

        if (options.MaximumPageNumbersToDisplay.HasValue && list.PageCount > options.MaximumPageNumbersToDisplay)
        {
            // cannot fit all pages into pager
            int maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;

            firstPageToDisplay = list.PageNumber - (maxPageNumbersToDisplay / 2);

            if (firstPageToDisplay < 1)
            {
                firstPageToDisplay = 1;
            }

            pageNumbersToDisplay = maxPageNumbersToDisplay;
            lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;

            if (lastPageToDisplay > list.PageCount)
            {
                firstPageToDisplay = list.PageCount - maxPageNumbersToDisplay + 1;
            }
        }

        //first
        if (firstPageToDisplay > 1)
        {
            listItemLinks.Add(First(list, generatePageUrl, options));
        }

        //previous
        if (!list.IsFirstPage)
        {
            listItemLinks.Add(Previous(list, generatePageUrl, options));
        }

        //text
        if (options.DisplayPageCountAndCurrentLocation)
        {
            listItemLinks.Add(PageCountAndLocationText(list, options));
        }

        //text
        if (options.DisplayItemSliceAndTotal && options.ItemSliceAndTotalPosition == ItemSliceAndTotalPosition.Start)
        {
            listItemLinks.Add(ItemSliceAndTotalText(list, options));
        }

        //page
        if (options.DisplayLinkToIndividualPages)
        {
            //if there are previous page numbers not displayed, show an ellipsis
            if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
            {
                listItemLinks.Add(PreviousEllipsis(list, generatePageUrl, options, firstPageToDisplay));
            }

            foreach (int i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
            {
                //show delimiter between page numbers
                if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
                {
                    listItemLinks.Add(WrapInListItem(options.DelimiterBetweenPageNumbers));
                }

                //show page number link
                listItemLinks.Add(Page(i, list, generatePageUrl, options));
            }

            //if there are subsequent page numbers not displayed, show an ellipsis
            if (options.DisplayEllipsesWhenNotShowingAllPageNumbers &&
                firstPageToDisplay + pageNumbersToDisplay - 1 < list.PageCount)
            {
                listItemLinks.Add(NextEllipsis(list, generatePageUrl, options, lastPageToDisplay));
            }
        }

        //next
        if (!list.IsLastPage)
        {
            listItemLinks.Add(Next(list, generatePageUrl, options));
        }

        //last
        if (lastPageToDisplay < list.PageCount)
        {
            listItemLinks.Add(Last(list, generatePageUrl, options));
        }

        //text
        if (options.DisplayItemSliceAndTotal && options.ItemSliceAndTotalPosition == ItemSliceAndTotalPosition.End)
        {
            listItemLinks.Add(ItemSliceAndTotalText(list, options));
        }

        if (listItemLinks.Any())
        {
            //append classes to all list item links
            foreach (TagBuilder li in listItemLinks)
            {
                foreach (string c in options.LiElementClasses ?? Enumerable.Empty<string>())
                {
                    li.AddCssClass(c);
                }
            }
        }

        //collapse all of the list items into one big string
        string listItemLinksString = listItemLinks.Aggregate(
            new StringBuilder(),
            (sb, listItem) => sb.Append(TagBuilderToString(listItem)),
            sb => sb.ToString());

        TagBuilder ul = tagBuilderFactory
            .Create("ul");

        AppendHtml(ul, listItemLinksString);

        foreach (string c in options.UlElementClasses ?? Enumerable.Empty<string>())
        {
            ul.AddCssClass(c);
        }

        TagBuilder outerDiv = tagBuilderFactory
            .Create("div");

        foreach (string c in options.ContainerDivClasses ?? Enumerable.Empty<string>())
        {
            outerDiv.AddCssClass(c);
        }

        AppendHtml(outerDiv, TagBuilderToString(ul));

        return TagBuilderToString(outerDiv);
    }

    #region Private methods

    private static void SetInnerText(TagBuilder tagBuilder, string innerText)
    {
        tagBuilder.InnerHtml.SetContent(innerText);
    }

    private static void AppendHtml(TagBuilder tagBuilder, string innerHtml)
    {
        tagBuilder.InnerHtml.AppendHtml(innerHtml);
    }

    private static string TagBuilderToString(TagBuilder tagBuilder)
    {
        HtmlEncoder encoder = HtmlEncoder.Create(new TextEncoderSettings());

        using TextWriter writer = new StringWriter() as TextWriter;
        tagBuilder.WriteTo(writer, encoder);

        return writer.ToString();
    }

    private TagBuilder WrapInListItem(string text)
    {
        TagBuilder li = tagBuilderFactory
            .Create("li");

        SetInnerText(li, text);

        return li;
    }

    private TagBuilder WrapInListItem(TagBuilder inner, PagedListRenderOptions options, params string[] classes)
    {
        TagBuilder li = tagBuilderFactory.Create("li");

        foreach (string @class in classes)
        {
            li.AddCssClass(@class);
        }

        if (options?.FunctionToTransformEachPageLink != null)
        {
            return options.FunctionToTransformEachPageLink(li, inner);
        }

        AppendHtml(li, TagBuilderToString(inner));

        return li;
    }

    private TagBuilder First(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
    {
        const int targetPageNumber = 1;
        TagBuilder first = tagBuilderFactory
            .Create("a");

        AppendHtml(first, string.Format(options.LinkToFirstPageFormat, targetPageNumber));

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            first.AddCssClass(c);
        }

        if (list.IsFirstPage)
        {
            return WrapInListItem(first, options, "paged-list-skip-to-first", "disabled");
        }

        first.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(first, options, "paged-list-skip-to-first");
    }

    private TagBuilder Previous(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
    {
        int targetPageNumber = list.PageNumber - 1;
        TagBuilder previous = tagBuilderFactory
            .Create("a");

        AppendHtml(previous, string.Format(options.LinkToPreviousPageFormat, targetPageNumber));

        previous.Attributes.Add("rel", "prev");

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            previous.AddCssClass(c);
        }

        if (!list.HasPreviousPage)
        {
            return WrapInListItem(previous, options, options.PreviousElementClass, "disabled");
        }

        previous.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(previous, options, options.PreviousElementClass);
    }

    private TagBuilder Page(int i, IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
    {
        Func<int, string> format = options.FunctionToDisplayEachPageNumber
                                   ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
        int targetPageNumber = i;
        TagBuilder page = i == list.PageNumber
            ? tagBuilderFactory
                .Create("span")
            : tagBuilderFactory
                .Create("a");

        SetInnerText(page, format(targetPageNumber));

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            page.AddCssClass(c);
        }

        if (i == list.PageNumber)
        {
            return WrapInListItem(page, options, options.ActiveLiElementClass);
        }

        page.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(page, options);
    }

    private TagBuilder Next(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
    {
        int targetPageNumber = list.PageNumber + 1;
        TagBuilder next = tagBuilderFactory
            .Create("a");

        AppendHtml(next, string.Format(options.LinkToNextPageFormat, targetPageNumber));

        next.Attributes.Add("rel", "next");

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            next.AddCssClass(c);
        }

        if (!list.HasNextPage)
        {
            return WrapInListItem(next, options, options.NextElementClass, "disabled");
        }

        next.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(next, options, options.NextElementClass);
    }

    private TagBuilder Last(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
    {
        int targetPageNumber = list.PageCount;
        TagBuilder last = tagBuilderFactory
            .Create("a");

        AppendHtml(last, string.Format(options.LinkToLastPageFormat, targetPageNumber));

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            last.AddCssClass(c);
        }

        if (list.IsLastPage)
        {
            return WrapInListItem(last, options, "paged-list-skip-to-last", "disabled");
        }

        last.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(last, options, "paged-list-skip-to-last");
    }

    private TagBuilder PageCountAndLocationText(IPagedList list, PagedListRenderOptions options)
    {
        TagBuilder text = tagBuilderFactory
            .Create("a");

        SetInnerText(text, string.Format(options.PageCountAndCurrentLocationFormat, list.PageNumber, list.PageCount));

        return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
    }

    private TagBuilder ItemSliceAndTotalText(IPagedList list, PagedListRenderOptions options)
    {
        TagBuilder text = tagBuilderFactory
            .Create("a");

        SetInnerText(text,
            string.Format(options.ItemSliceAndTotalFormat, list.FirstItemOnPage, list.LastItemOnPage,
                list.TotalItemCount));

        return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
    }

    private TagBuilder PreviousEllipsis(IPagedList list, Func<int, string> generatePageUrl,
        PagedListRenderOptions options, int firstPageToDisplay)
    {
        TagBuilder previous = tagBuilderFactory
            .Create("a");

        AppendHtml(previous, options.EllipsesFormat);

        previous.Attributes.Add("rel", "prev");
        previous.AddCssClass("paged-list-skip-to-previous");

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            previous.AddCssClass(c);
        }

        if (!list.HasPreviousPage)
        {
            return WrapInListItem(previous, options, options.EllipsesElementClass, "disabled");
        }

        int targetPageNumber = firstPageToDisplay - 1;

        previous.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(previous, options, options.EllipsesElementClass);
    }

    private TagBuilder NextEllipsis(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options,
        int lastPageToDisplay)
    {
        TagBuilder next = tagBuilderFactory
            .Create("a");

        AppendHtml(next, options.EllipsesFormat);

        next.Attributes.Add("rel", "next");
        next.AddCssClass("paged-list-skip-to-next");

        foreach (string c in options.PageClasses ?? Enumerable.Empty<string>())
        {
            next.AddCssClass(c);
        }

        if (!list.HasNextPage)
        {
            return WrapInListItem(next, options, options.EllipsesElementClass, "disabled");
        }

        int targetPageNumber = lastPageToDisplay + 1;

        next.Attributes.Add("href", generatePageUrl(targetPageNumber));

        return WrapInListItem(next, options, options.EllipsesElementClass);
    }

    #endregion Private methods
}