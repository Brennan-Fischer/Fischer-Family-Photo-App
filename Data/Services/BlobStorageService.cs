using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;

namespace Fischbowl_Project.Data.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;

        public BlobStorageService(string connectionString)
        {
            _connectionString = connectionString;
        }

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
