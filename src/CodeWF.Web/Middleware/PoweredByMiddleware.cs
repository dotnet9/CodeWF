namespace CodeWF.Web.Middleware;

public class PoweredByMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext httpContext)
    {
        httpContext.Response.Headers["X-Powered-By"] = $"CodeWF {Helper.AppVersion}";
        return next.Invoke(httpContext);
    }
}