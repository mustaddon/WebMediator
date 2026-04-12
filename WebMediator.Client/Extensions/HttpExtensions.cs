using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace WebMediator.Client.Extensions;

internal static class HttpExtensions
{
    public static Task<HttpResponseMessage> PostExt(this HttpClient client, string uri, HttpContent? content, CancellationToken cancellationToken)
    {
        return client.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = content,
        }, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsStreamExt(this HttpClient client, string uri, object? request, CancellationToken cancellationToken)
    {
        if (request is not Stream stream)
            throw new ArgumentNullException(nameof(request));

        return client.PostExt(uri, new StreamContent(stream), cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsJsonExt(this HttpClient client, string uri, object? request, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        var content = JsonContent.Create(request, options: jsonOptions);
#else
        var content = new StringContent(JsonSerializer.Serialize(request, jsonOptions), System.Text.Encoding.UTF8, "application/json");
#endif
        return client.PostExt(uri, content, cancellationToken);
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

    public static bool IsEventStream(this MediaTypeHeaderValue? value)
    {
        return string.Equals(value?.MediaType, "text/event-stream", StringComparison.InvariantCultureIgnoreCase);
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


    public static object ToAsyncEnumerable(this HttpResponseMessage response, Type itemType, JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        var method = itemType.TryGetSseItemType(out var sseItemType)
            ? _toAsyncEnumerableSseMethod.MakeGenericMethod(sseItemType)
            : _toAsyncEnumerableMethod.MakeGenericMethod(itemType);

        return method.Invoke(null, [response, options, cancellationToken])!;
    }

    static readonly MethodInfo _toAsyncEnumerableMethod = new Func<HttpResponseMessage, JsonSerializerOptions, CancellationToken, IAsyncEnumerable<object?>>(ToAsyncEnumerable<object>).Method.GetGenericMethodDefinition();
    static readonly MethodInfo _toAsyncEnumerableSseMethod = new Func<HttpResponseMessage, JsonSerializerOptions, CancellationToken, IAsyncEnumerable<SseItem<object?>>>(ToAsyncEnumerableSse<object>).Method.GetGenericMethodDefinition();

    static async IAsyncEnumerable<T?> ToAsyncEnumerable<T>(this HttpResponseMessage response, JsonSerializerOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var sse in ToAsyncEnumerableSse<T>(response, options, cancellationToken))
            yield return sse.Data;
    }

    static async IAsyncEnumerable<SseItem<T?>> ToAsyncEnumerableSse<T>(this HttpResponseMessage response, JsonSerializerOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            await foreach (var sse in SseParser.Create(stream).EnumerateAsync(cancellationToken))
                yield return new(JsonSerializer.Deserialize<T>(sse.Data, options), sse.EventType)
                {
                    EventId = sse.EventId,
                    ReconnectionInterval = sse.ReconnectionInterval,
                };
        }
        finally
        {
            response.Dispose();
        }
    }
}
