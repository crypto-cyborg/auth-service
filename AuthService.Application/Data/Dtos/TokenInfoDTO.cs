﻿namespace AuthService.Application.Data.Dtos
{
    public class TokenInfoDTO
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}
