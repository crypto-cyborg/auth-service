namespace AuthService.Core.Exceptions;

public class AuthServiceExceptions(string message, AuthServiceExceptionTypes type)
    : Exception(message)
{
    public AuthServiceExceptionTypes Types { get; set; } = type;
}
