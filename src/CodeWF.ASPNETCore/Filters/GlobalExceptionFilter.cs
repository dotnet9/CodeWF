using Microsoft.Extensions.Hosting;

namespace CodeWF.ASPNETCore.Filters;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
    : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var message = env.IsDevelopment() ? context.Exception.ToString() : "程序中出现未处理异常";
        context.Result = new ObjectResult(ResponseResult<object>.GetError(HttpStatusCode.InternalServerError, message));
        context.ExceptionHandled = true;
        logger.LogError(context.Exception, message);
        await Task.CompletedTask;
    }
}