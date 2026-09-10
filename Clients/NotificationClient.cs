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
        await _httpClient.PostAsync(
            "/api/Notification/sendNotification",
            null);
    }
}