using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class BinanceService(IConfiguration configuration, HttpClient httpClient) : IBinanceService
{
    public async Task<bool> ValidateKeys(string apiKey, string secretKey)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var query = $"timestamp={timestamp}";
            var signature = Sign(query, secretKey);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"account?{query}&signature={signature}"
            );

            request.Headers.Add("X-MBX-APIKEY", apiKey);

            var response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    [Pure]
    private static string Sign(string query, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(query));

        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}