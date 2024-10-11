using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text.Json;

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
                BaseAddress = new(_configuration.GetSection("Services")["UserService"])
            };
        }

        public async Task<User> GetUser(Guid? id)
        {
            var response = await _httpClient.GetAsync($"users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot proceed request. {nameof(GetUser)}");
            }

            var dataStream = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(dataStream)
                ?? throw new Exception("Unable to serialize");

            return user;
        }

        public async Task<Guid?> GetUserId(string username)
        {
            var response = await _httpClient.GetAsync($"users/{username}/exists");

            Console.WriteLine(_httpClient.BaseAddress);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request failed with status code {response.StatusCode}. {nameof(GetUserId)}. Response: {errorContent}");
            }

            var dataStream = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ExistsDto>(dataStream)
                ?? throw new Exception("Unable to serialize");

            if (!data.Found)
            {
                return null;
            }

            return data.Data;
        }
    }
}
