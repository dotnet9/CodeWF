namespace CodeWF.Tools.Module.Test.MediatR.RequestHandlers;

public class TestHandler : IRequestHandler<TestRequest, string>
{
    public async Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult($"模块【Test】Request处理程序：Args = {request.Args}, Now = {DateTime.Now}");
    }
}