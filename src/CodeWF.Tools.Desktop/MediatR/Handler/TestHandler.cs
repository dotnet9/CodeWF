using CodeWF.Tools.MediatR.Requests;
using MediatR;

namespace CodeWF.Tools.Desktop.MediatR.Handler;

public class TestHandler : IRequestHandler<TestRequest, string>
{
    public async Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult($"Args = {request.Args}, Now = {DateTime.Now}");
    }
}