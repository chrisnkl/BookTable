using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BookTable.Services.impl
{
    /// <summary>
    /// Implements the Static Content Hosting Pattern using Azure Blob Storage.
    /// When using the Azurite local emulator, set connectionString to "UseDevelopmentStorage=true".
    /// This service stores static assets (e.g. restaurant floor plans, seating charts)
    /// directly in blob storage, offloading them from the web application server.
    /// </summary>
    public class StaticContentService : IStaticContentService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public StaticContentService(string connectionString, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName;
        }

        /// <summary>
        /// Ensure the storage container exists with public read access.
        /// Must be called once at application startup.
        /// </summary>
        public async Task InitializeContainerAsync()
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        /// <summary>
        /// Uploads a file to blob storage and returns its public URL.
        /// </summary>
        public async Task<string> UploadFileAsync(string blobName, Stream content, string contentType)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blob.UploadAsync(content, options);

            return blob.Uri.ToString();
        }

        /// <summary>
        /// Returns the public URL of an existing blob, or null if it does not exist.
        /// </summary>
        public async Task<string?> GetBlobUrlAsync(string blobName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobName);

            bool exists = await blob.ExistsAsync();
            return exists ? blob.Uri.ToString() : null;
        }

        /// <summary>
        /// Lists all blob names in the container.
        /// </summary>
        public async Task<List<string>> ListBlobsAsync()
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobs = new List<string>();

            await foreach (var blob in container.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }

            return blobs;
        }

        /// <summary>
        /// Deletes a blob from storage.
        /// </summary>
        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobName);
            var response = await blob.DeleteIfExistsAsync();
            return response.Value;
        }
    }
}
