using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.IO;
using System.Threading.Tasks;

namespace Fischbowl_Project.Data.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;

        public BlobStorageService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Method to generate a SAS URL for a blob
        public string GetBlobSasUrl(string containerName, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

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

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            else
            {
                throw new InvalidOperationException("Cannot generate SAS URL for this blob.");
            }
        }

        // Method to retrieve a blob as a byte array
        public async Task<byte[]> GetBlobAsync(string containerName, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var ms = new MemoryStream())
            {
                await blobClient.DownloadToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
