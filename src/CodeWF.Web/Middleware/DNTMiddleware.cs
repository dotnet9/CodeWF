using Microsoft.Extensions.Primitives;

namespace CodeWF.Web.Middleware;

public class DNTMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext httpContext)
    {
        StringValues dntFlag = httpContext.Request.Headers["DNT"];
        bool dnt = !string.IsNullOrWhiteSpace(dntFlag) && dntFlag == "1";

        httpContext.Items["DNT"] = dnt;

        return next.Invoke(httpContext);
    }
}