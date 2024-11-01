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
        catch (AuthServiceExceptions ex)
        {
            await HandleException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private Task HandleException(HttpContext context, Exception ex)
    {
        var code = HttpStatusCode.InternalServerError;

        if (ex is AuthServiceExceptions authEx)
        {
            code = authEx.Types switch
            {
                AuthServiceExceptionTypes.SERIALIZATION_ERROR => HttpStatusCode.BadRequest,
                AuthServiceExceptionTypes.DESEREALIZATION_ERROR => HttpStatusCode.BadRequest,
                AuthServiceExceptionTypes.USER_NOT_FOUND => HttpStatusCode.NotFound,
                AuthServiceExceptionTypes.INVALID_PASSWORD => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };
        }

        var result = JsonConvert.SerializeObject(new { ex.Message, code });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}