using MediatR;

namespace CodeWF.Tools.MediatR.Command;

public class CopyToClipboardCommand : IRequest<bool>
{
    public string Content { get; set; } = null!;
}