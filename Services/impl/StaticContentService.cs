using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BookTable.Services.impl
{
    public class StaticContentService : IStaticContentService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public StaticContentService(string connectionString, string containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = containerName;
        }
        
        public async Task InitializeContainerAsync()
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        
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
        
        public async Task<string?> GetBlobUrlAsync(string blobName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobName);

            bool exists = await blob.ExistsAsync();
            return exists ? blob.Uri.ToString() : null;
        }
        
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
        
        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobName);
            var response = await blob.DeleteIfExistsAsync();
            return response.Value;
        }
    }
}
