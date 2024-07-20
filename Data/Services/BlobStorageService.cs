namespace Fischbowl_Project.Data.Services
{
	// BlobStorageService.cs
	using Azure.Storage.Blobs;
	using System.IO;
	using System.Threading.Tasks;

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
			// Create a BlobServiceClient to interact with the Blob service
			BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

			// Get a reference to a specific container
			BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

			// Get a reference to a specific blob
			BlobClient blobClient = containerClient.GetBlobClient(blobName);

			// Download the blob content to a memory stream
			using (MemoryStream ms = new MemoryStream())
			{
				await blobClient.DownloadToAsync(ms);
				return ms.ToArray(); // Return the content as a byte array
			}
		}
	}

}
