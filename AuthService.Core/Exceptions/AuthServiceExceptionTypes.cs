namespace AuthService.Core.Exceptions;

public enum AuthServiceExceptionTypes
{
    SERIALIZATION_ERROR,
    DESEREALIZATION_ERROR,
    USER_NOT_FOUND,
    INVALID_PASSWORD,
    IVALID_COOKIE_CONFIGURATION,
    INVALID_KEYS,
    TOKEN_NOT_FOUND
}
