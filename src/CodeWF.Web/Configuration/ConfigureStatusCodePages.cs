namespace CodeWF.Web.Configuration;

public class ConfigureStatusCodePages
{
    public static Func<StatusCodeContext, Task> Handler => async context =>
    {
        int statusCode = context.HttpContext.Response.StatusCode;
        string requestId = context.HttpContext.TraceIdentifier;
        string description = ReasonPhrases.GetReasonPhrase(context.HttpContext.Response.StatusCode);

        await context.HttpContext.Response.WriteAsJsonAsync(new { statusCode, requestId, description },
            context.HttpContext.RequestAborted);
    };
}