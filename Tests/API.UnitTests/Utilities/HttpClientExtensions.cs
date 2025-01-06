using System.Net.Http.Json;

namespace API.UnitTests.Utilities;

public static class HttpClientExtensions
{
    public static async Task<T?> GetAndDeserialize<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}