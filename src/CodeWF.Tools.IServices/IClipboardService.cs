namespace CodeWF.Tools.IServices;

public interface IClipboardService
{
    void SetHostWindow(TopLevel window);

    Task CopyToAsync(string content);
}