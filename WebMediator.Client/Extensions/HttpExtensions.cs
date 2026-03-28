namespace WebMediator.Client.Extensions;

internal static class HttpExtensions
{
    public static Task<HttpResponseMessage> PostAsStream(this HttpClient client, string uri, object? request, CancellationToken cancellationToken)
    {
        if (request is not Stream stream)
            throw new ArgumentNullException(nameof(request));

        var content = new StreamContent(stream);

        return client.PostAsync(uri, content, cancellationToken);
    }

    public static void EnsureSuccessStatusCodeDisposable(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        response.Dispose();
        throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase})");
    }

    public static bool IsPlainText(this MediaTypeHeaderValue? value)
    {
        return string.Equals(value?.MediaType, "text/plain", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsJson(this MediaTypeHeaderValue? value)
    {
        return string.Equals(value?.MediaType, "application/json", StringComparison.InvariantCultureIgnoreCase);
    }


#if NETSTANDARD2_0
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task<Stream> ReadAsStreamAsync(this HttpContent httpContent, CancellationToken cancellationToken) => httpContent.ReadAsStreamAsync();
    
    public static Task<string> ReadAsStringAsync(this HttpContent httpContent, CancellationToken cancellationToken) => httpContent.ReadAsStringAsync();
    
    public static async Task<object?> ReadFromJsonAsync(this HttpContent content, Type type, JsonSerializerOptions? options, CancellationToken cancellationToken = default)
    {
        using var stream = await content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync(stream, type, options, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(value, options));
        content.Headers.Add("Content-Type", "application/json");
        return client.PostAsync(requestUri, content, cancellationToken);
    }
#pragma warning restore IDE0060 // Remove unused parameter
#endif

}
