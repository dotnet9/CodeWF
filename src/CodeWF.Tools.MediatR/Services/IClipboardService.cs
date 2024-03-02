namespace CodeWF.Tools.MediatR.Services;

public interface IClipboardService
{
    void SetHostWindow(TopLevel window);

    Task CopyToAsync(string content);
}