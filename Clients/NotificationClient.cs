namespace BookTable.Clients;

public class NotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendNotificationAsync()
    {
        var response = await _httpClient.PostAsync(
            "/api/Notification/sendNotification",
            null);
        
        response.EnsureSuccessStatusCode();
        
    }
}