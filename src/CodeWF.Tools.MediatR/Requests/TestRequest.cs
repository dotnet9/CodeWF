using MediatR;

namespace CodeWF.Tools.MediatR.Requests;

public class TestRequest : IRequest<string>
{
    public string? Args { get; set; }
}