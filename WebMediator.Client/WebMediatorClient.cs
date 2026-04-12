using System.Data.Common;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using TypeSerialization.Json;

namespace WebMediator.Client;

public class WebMediatorClient : IWebMediatorClient, IDisposable
{
    public WebMediatorClient(string address, WebMediatorClientSettings? settings = null)
    {
        settings ??= new();
        _client = new(() => CreateHttpClient(address, settings));
        EventStreamOptions = settings.EventStreamOptions ?? new();

        TypeDeserializer = settings.TypeDeserializer ?? TypeDeserializer.Default;
        JsonOptions = new(settings.JsonSerializerOptions)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        JsonOptions.Converters.TryAdd(new JsonTypeConverter(TypeDeserializer));

        JsonOptionsIndented = new(JsonOptions)
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    private readonly Lazy<HttpClient> _client;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly JsonSerializerOptions JsonOptionsIndented;
    protected readonly EventStreamOptions EventStreamOptions;

    public TypeDeserializer TypeDeserializer { get; }

    public Uri BaseAddress => _client.Value.BaseAddress!;
    public HttpRequestHeaders DefaultRequestHeaders => _client.Value.DefaultRequestHeaders;

    public virtual void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();

        GC.SuppressFinalize(this);
    }

    public virtual string GetUrl(Type requestType, object? request = null)
    {
        if (typeof(Stream).IsAssignableFrom(requestType) || requestType.GetStreamProperty() != null)
            throw new InvalidOperationException("The data contains stream property and cannot be presented as a simple URL.");

        if(request == null)
            return _client.Value.BaseAddress + requestType.Serialize();

        return string.Format("{0}{1}?data={2}", _client.Value.BaseAddress, requestType.Serialize(), Uri.EscapeDataString(JsonSerializer.Serialize(request, JsonOptionsIndented)));
    }

    public virtual Task<object?> Send(Type requestType, object? request, Type resultType, CancellationToken cancellationToken = default)
    {
        return GetResult(CreateRequest(requestType, request, cancellationToken), resultType, cancellationToken);
    }

    public virtual async IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(Type requestType, object? request = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        var reconnections = 0U;

        while (!cancellationToken.IsCancellationRequested)
        {
            Exception? error = null;
            var requestTask = CreateRequest(requestType, request, cancellationToken);
            var result = await this.TrySafe(
                x => x.GetResult(requestTask, typeof(IAsyncEnumerable<SseItem<TResult>>), cancellationToken), 
                e => error = e);

            if (error == null)
            {
                await using var enumerator = ((IAsyncEnumerable<SseItem<TResult>>)result!).GetAsyncEnumerator(cancellationToken);

                while (await enumerator.TrySafe(async x => await x.MoveNextAsync(), e => error = e))
                    yield return enumerator.Current;
            }

            if (reconnections >= EventStreamOptions.ReconnectionRetriesLimit)
                throw new Exception($"The limit on the number of reconnection attempts has been reached. (RequestUri: {(await requestTask).RequestMessage?.RequestUri})", error);

            if (EventStreamOptions.ReconnectionDelay == Timeout.InfiniteTimeSpan || EventStreamOptions.ReconnectionDelay < TimeSpan.Zero)
            {
                if (error != null)
                    throw new Exception($"Event stream error. (RequestUri: {(await requestTask).RequestMessage?.RequestUri})", error);

                break;
            }

            await Task.Delay(EventStreamOptions.ReconnectionDelay, cancellationToken);
            reconnections++;
        }
    }

    protected virtual Task<HttpResponseMessage> CreateRequest(Type requestType, object? request, CancellationToken cancellationToken)
    {
        if (typeof(Stream).IsAssignableFrom(requestType))
            return _client.Value.PostAsStreamExt(typeof(Stream).Serialize(), request, cancellationToken);

        var streamProp = requestType.GetStreamProperty();

        if (streamProp != null)
        {
            var stream = streamProp.GetValue(request);
            streamProp.SetValue(request, null);
            var uri = string.Format("{0}?data={1}", requestType.Serialize(), Uri.EscapeDataString(JsonSerializer.Serialize(request, JsonOptionsIndented)));
            streamProp.SetValue(request, stream);

            if (stream == null)
                return _client.Value.PostExt(uri, null, cancellationToken);

            return _client.Value.PostAsStreamExt(uri, stream, cancellationToken);
        }

        return _client.Value.PostAsJsonExt(requestType.Serialize(), request, JsonOptions, cancellationToken);
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

        if (response.Content.Headers.ContentType.IsEventStream())
            return response.ToAsyncEnumerable(resultType.GetAsyncEnumerableItemType() ?? typeof(object), JsonOptions, cancellationToken);

        var streamProp = resultType.GetStreamProperty();

        if (streamProp != null && response.Headers.TryGetValues("data", out var dataHeaders))
        {
            var result = JsonSerializer.Deserialize(Convert.FromBase64String(dataHeaders.First()), resultType, JsonOptions);
            streamProp.SetValue(result, new HttpStream(await response.Content.ReadAsStreamAsync(cancellationToken), response.Dispose));
            return result;
        }

        if (!response.Content.Headers.ContentType.IsJson())
            return new HttpStream(await response.Content.ReadAsStreamAsync(cancellationToken), response.Dispose);

        using (response)
        {
            return await response.Content.ReadFromJsonAsync(resultType, JsonOptions, cancellationToken);
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
