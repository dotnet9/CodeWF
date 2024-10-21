namespace WebSite.Middlewares;

public class CustomRoutingMiddleware
{
    private readonly RequestDelegate _next;

    public CustomRoutingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestedPath = context.Request.Path.Value;

        var filePath = Path.Combine(AppContext.BaseDirectory, requestedPath.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", requestedPath.TrimStart('/'));
        }
        if(File.Exists(filePath))
        {
            var fileContent = await File.ReadAllBytesAsync(filePath);
            context.Response.ContentType = GetContentType(requestedPath);
            await context.Response.Body.WriteAsync(fileContent, 0, fileContent.Length);
        }
        else
        {
            // 如果文件不存在，继续执行后续的中间件和路由处理
            await _next(context);
        }
    }

    private string GetContentType(string path)
    {
        var extension = Path.GetExtension(path);
        switch (extension)
        {
            case ".txt":
                return "text/plain";
            // 根据需要添加更多文件类型的判断和对应的 Content-Type
            default:
                return "application/octet-stream";
        }
    }
}