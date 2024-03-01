using Avalonia.Controls;

namespace CodeWF.Tools.MediatR.Command;

public interface IClipboardService
{
    void SetHostWindow(TopLevel window);

    Task CopyToAsync(string content);
}