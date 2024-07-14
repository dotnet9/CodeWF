using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CodeWF.Shared.Highlight
{
    public interface IPrismHighlighter
    {
        ValueTask<MarkupString> HighlightAsync(string code, string language);

        Task HighlightAllAsync();
    }
}