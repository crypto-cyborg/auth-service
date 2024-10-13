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

    private Task HandleException(HttpContext context, AuthServiceExceptions ex)
    {
        var code = HttpStatusCode.InternalServerError;

        switch (ex.Types)
        {
            case AuthServiceExceptionTypes.SERIALIZATION_ERROR:
                break;

            case AuthServiceExceptionTypes.DESEREALIZATION_ERROR:
                break;

            case AuthServiceExceptionTypes.USER_NOT_FOUND:
                code = HttpStatusCode.NotFound;
                break;
        }

        var result = JsonConvert.SerializeObject(new { ex.Message, code });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private Task HandleException(HttpContext context, Exception ex)
    {
        var code = HttpStatusCode.InternalServerError;

        var result = JsonConvert.SerializeObject(new { ex.Message, code });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
