using CodeWF.Tools.MediatR.Command;
using MediatR;

namespace CodeWF.Tools.Desktop.MediatR;

internal class CopyToClipboardCommandHandle : IRequestHandler<CopyToClipboardCommand, bool>
{
    public async Task<bool> Handle(CopyToClipboardCommand request, CancellationToken cancellationToken)
    {
        return true;
    }
}