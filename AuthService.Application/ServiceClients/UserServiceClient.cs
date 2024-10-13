using System.Net.Http.Json;
using System.Runtime.Serialization;
using AuthService.Application.Data.Dtos;
using AuthService.Core.Exceptions;
using AuthService.Core.Models;
using AuthService.Persistence.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AuthService.Application.ServiceClients
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserServiceClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new()
            {
                BaseAddress = new(_configuration.GetSection("Services")["UserService"]),
            };
        }

        public async Task<User> GetUser(string username)
        {
            var id = await GetUserId(username);
            var response = await _httpClient.GetAsync($"users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot proceed request. {nameof(GetUser)}");
            }

            var dataStream = await response.Content.ReadAsStringAsync();
            var user =
                JsonConvert.DeserializeObject<User>(dataStream)
                ?? throw new AuthServiceExceptions(
                    $"Internal server error",
                    AuthServiceExceptionTypes.SERIALIZATION_ERROR
                );

            return user;
        }

        private async Task<Guid?> GetUserId(string username)
        {
            var response = await _httpClient.GetAsync($"users/{username}/exists");

            Console.WriteLine(_httpClient.BaseAddress);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Request failed with status code {response.StatusCode}. {nameof(GetUserId)}. Response: {errorContent}"
                );
            }

            var dataStream = await response.Content.ReadAsStringAsync();
            var data =
                JsonConvert.DeserializeObject<ExistsDto>(dataStream)
                ?? throw new Exception("Unable to serialize");

            if (!data.Found)
            {
                throw new AuthServiceExceptions(
                    "User not found",
                    AuthServiceExceptionTypes.USER_NOT_FOUND
                );
            }

            return data.Data;
        }

        public async Task<User> CreateUser(SignUpDTO request)
        {
            var body = JsonContent.Create(request);
            var response = await _httpClient.PostAsync("users", body);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var dataString = await response.Content.ReadAsStringAsync();
            var data =
                JsonConvert.DeserializeObject<User>(dataString)
                ?? throw new AuthServiceExceptions(
                    $"Internal server error",
                    AuthServiceExceptionTypes.DESEREALIZATION_ERROR
                );

            return data;
        }

        public async Task<User> UpdateUser(User request)
        {
            var body = JsonContent.Create(request);
            var response = await _httpClient.PatchAsync($"users/{request.Id}", body);

            System.Console.WriteLine(body.Value);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to update refresh token for user with ID {request.Id}. Status code: {response.StatusCode}"
                );
            }

            var dataString = await response.Content.ReadAsStringAsync();
            var updatedUser =
                JsonConvert.DeserializeObject<User>(dataString)
                ?? throw new AuthServiceExceptions(
                    $"Internal server error",
                    AuthServiceExceptionTypes.DESEREALIZATION_ERROR
                );

            return updatedUser;
        }
    }
}
