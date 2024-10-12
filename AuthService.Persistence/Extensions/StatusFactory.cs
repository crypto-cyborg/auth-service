using System;
using AuthService.Core.Models;

namespace AuthService.Persistence.Extensions;

public static class StatusFactory
{
    public static Status Create(int code, string message, bool isError = false)
    {
        return new Status
        {
            Code = code,
            Message = message,
            IsError = isError,
        };
    }
}
