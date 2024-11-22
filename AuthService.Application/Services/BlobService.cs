using AuthService.Application.Services.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class BlobService(IConfiguration configuration) : IBlobService
{
    public async Task<string> UploadImage(IFormFile image)
    {
        var blobServiceClient = new BlobServiceClient(configuration["Azure:StorageConnectionString"]);
        var containerClient = blobServiceClient.GetBlobContainerClient(configuration["Azure:ContainerName"]);

        await containerClient.CreateIfNotExistsAsync();
        await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

        var blobName = Guid.NewGuid() + Path.GetExtension(image.FileName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = image.OpenReadStream();
        await blobClient.UploadAsync(stream, true);

        var fileUrl = blobClient.Uri.ToString();

        return fileUrl; 
    }
}