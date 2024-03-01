using Avalonia.Input;
using Avalonia.Input.Platform;
using CodeWF.Tools.MediatR.Command;

namespace CodeWF.Tools.Desktop.Services;

internal class ClipboardService : IClipboardService
{
    private IClipboard? _clipboard;


    public void SetHostWindow(TopLevel hostWindow)
    {
        _clipboard = hostWindow.Clipboard;
    }

    public async Task CopyToAsync(string content)
    {
        if (_clipboard is not null)
        {
            var dataObject = new DataObject();
            dataObject.Set(DataFormats.Text, content);
            await _clipboard.SetDataObjectAsync(dataObject);
        }
    }
}