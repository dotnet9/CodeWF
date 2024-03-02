namespace CodeWF.Tools.Desktop.MediatR.RequestHandlers;

public class TestHandler : IRequestHandler<TestRequest, string>
{
    public async Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult($"主工程处理程序：Args = {request.Args}, Now = {DateTime.Now}");
    }
}