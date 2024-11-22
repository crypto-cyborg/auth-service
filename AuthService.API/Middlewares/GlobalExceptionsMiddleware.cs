using System.Net;
using AuthService.Core.Exceptions;
using Newtonsoft.Json;

namespace AuthService.API.Middlewares;

public class GlobalExceptionsMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private record Response(string Message, int code);

    private Task HandleException(HttpContext context, Exception ex)
    {
        try
        {
            if (JsonConvert.DeserializeObject<Response>(ex.Message) is Response)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                return context.Response.WriteAsync(ex.Message);
            }
        }
        catch { }

        var code = HttpStatusCode.InternalServerError;

        if (ex is AuthServiceExceptions authEx)
        {
            code = authEx.Types switch
            {
                AuthServiceExceptionTypes.SERIALIZATION_ERROR => HttpStatusCode.BadRequest,
                AuthServiceExceptionTypes.DESEREALIZATION_ERROR => HttpStatusCode.BadRequest,
                AuthServiceExceptionTypes.USER_NOT_FOUND => HttpStatusCode.NotFound,
                AuthServiceExceptionTypes.INVALID_PASSWORD => HttpStatusCode.Forbidden,
                AuthServiceExceptionTypes.INVALID_KEYS => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };
        }

        var result = JsonConvert.SerializeObject(new { ex, code });
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
