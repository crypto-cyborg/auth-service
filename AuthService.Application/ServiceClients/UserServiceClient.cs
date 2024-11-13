using System.Net.Http.Json;
using AuthService.Application.Data.Dtos;
using AuthService.Core.Exceptions;
using AuthService.Core.Models;
using Newtonsoft.Json;

namespace AuthService.Application.ServiceClients
{
    public class UserServiceClient(HttpClient httpClient)
    {
        public async Task<User> GetUser(Guid id)
        {
            var response = await httpClient.GetAsync($"users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
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

        public async Task<User> GetUser(string username)
        {
            var id = await GetUserId(username) ?? throw new Exception();
            var user = await GetUser(id: id);

            return user;
        }

        private async Task<Guid?> GetUserId(string username)
        {
            var response = await httpClient.GetAsync($"users/{username}/exists");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
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
            var response = await httpClient.PostAsync("users", body);

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
            var response = await httpClient.PatchAsync($"users/{request.Id}", body);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
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
