namespace BookTable.Services
{
    public interface IStaticContentService
    {
        Task<string> UploadFileAsync(string blobName, Stream content, string contentType);
        Task<string?> GetBlobUrlAsync(string blobName);
        Task<List<string>> ListBlobsAsync();
        Task<bool> DeleteBlobAsync(string blobName);
        Task InitializeContainerAsync();
    }
}
