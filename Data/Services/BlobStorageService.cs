using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.Threading.Tasks;

namespace Fischbowl_Project.Data.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;

        // Constructor to initialize the service with the connection string
        public BlobStorageService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Method to retrieve a blob as a byte array
        public async Task<byte[]> GetBlobAsync(string containerName, string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using (MemoryStream ms = new MemoryStream())
            {
                await blobClient.DownloadToAsync(ms);
                return ms.ToArray();
            }
        }

        // Method to generate a SAS URL for a blob
        public string GetBlobSasUrl(string containerName, string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // Set the expiration time for the SAS URL
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            else
            {
                throw new InvalidOperationException("Cannot generate SAS URL for this blob.");
            }
        }
    }
}
