using TypeSerialization.Json;

namespace WebMediator.Client;

public class WebMediatorClient : IWebMediatorClient, IDisposable
{
    public WebMediatorClient(string address, WebMediatorClientSettings? settings = null)
    {
        settings ??= new();
        _client = new(() => CreateHttpClient(address, settings));

        TypeDeserializer = settings.TypeDeserializer ?? TypeDeserializer.Default;
        JsonSerializerOptions = settings.JsonSerializerOptions;

        if (!JsonSerializerOptions.Converters.Any(x => typeof(JsonTypeConverter).IsAssignableFrom(x.GetType())))
            JsonSerializerOptions.Converters.Add(new JsonTypeConverter(TypeDeserializer));
    }

    private readonly Lazy<HttpClient> _client;
    protected readonly JsonSerializerOptions JsonSerializerOptions;

    public TypeDeserializer TypeDeserializer { get; }

    public HttpRequestHeaders DefaultRequestHeaders => _client.Value.DefaultRequestHeaders;

    public virtual void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();

        GC.SuppressFinalize(this);
    }

    public virtual Task<object?> Send(object? request, Type requestType, Type resultType, CancellationToken cancellationToken = default)
    {
        return GetResult(CreateRequest(request, requestType, cancellationToken), resultType, cancellationToken);
    }

    protected virtual Task<HttpResponseMessage> CreateRequest(object? request, Type requestType, CancellationToken cancellationToken)
    {
        if (typeof(Stream).IsAssignableFrom(requestType))
            return _client.Value.PostAsStream(typeof(Stream).Serialize(), request, cancellationToken);

        var streamProp = requestType.GetStreamProperty();

        if (streamProp != null)
        {
            var stream = streamProp.GetValue(request);
            streamProp.SetValue(request, null);
            var uri = string.Format("{0}?data={1}", requestType.Serialize(), Uri.EscapeDataString(JsonSerializer.Serialize(request, JsonSerializerOptions)));
            streamProp.SetValue(request, stream);

            if (stream == null)
                return _client.Value.PostAsync(uri, null, cancellationToken);

            return _client.Value.PostAsStream(uri, stream, cancellationToken);
        }

        return _client.Value.PostAsJsonAsync(requestType.Serialize(), request, JsonSerializerOptions, cancellationToken);
    }

    protected virtual async Task<object?> GetResult(Task<HttpResponseMessage> requestTask, Type resultType, CancellationToken cancellationToken)
    {
        var response = await requestTask;

        response.EnsureSuccessStatusCodeDisposable();

        if (response.StatusCode == HttpStatusCode.NoContent || resultType == typeof(void))
        {
            response.Dispose();
            return null;
        }

        if ((resultType == typeof(object) || resultType.IsAbstractPlus())
            && response.Headers.TryGetValues("data-type", out var dataTypeHeaders))
            resultType = TypeDeserializer.Deserialize(dataTypeHeaders.First())!;

        if (resultType == typeof(Stream))
            return new HttpStream(await response.Content.ReadAsStreamAsync(cancellationToken), response.Dispose);

        var streamProp = resultType.GetStreamProperty();

        if (streamProp != null && response.Headers.TryGetValues("data", out var dataHeaders))
        {
            var result = JsonSerializer.Deserialize(Convert.FromBase64String(dataHeaders.First()), resultType, JsonSerializerOptions);
            streamProp.SetValue(result, new HttpStream(await response.Content.ReadAsStreamAsync(cancellationToken), response.Dispose));
            return result;
        }

        if (!response.Content.Headers.ContentType.IsJson())
            return new HttpStream(await response.Content.ReadAsStreamAsync(cancellationToken), response.Dispose);

        using (response)
        {
            return await response.Content.ReadFromJsonAsync(resultType, JsonSerializerOptions, cancellationToken);
        }
    }

    protected virtual HttpClient CreateHttpClient(string address, WebMediatorClientSettings settings)
    {
        if (!address.EndsWith("/"))
            address += "/";

        var client = settings.HttpHandler != null
            ? new HttpClient(settings.HttpHandler)
            : new HttpClient();

        client.BaseAddress = new Uri(address);

        if (settings.HttpHeaders != null)
            foreach (var kvp in settings.HttpHeaders)
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);

        return client;
    }
}
