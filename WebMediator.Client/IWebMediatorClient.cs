using System.Net.ServerSentEvents;

namespace WebMediator.Client;

public interface IWebMediatorClient
{
    string GetUrl(Type requestType, object? request = null);

    Task<object?> Send(Type requestType, object? request, Type resultType, CancellationToken cancellationToken = default);

    IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(Type requestType, object? request = null, CancellationToken cancellationToken = default);
}

public class EventStreamOptions
{
    public uint? ReconnectionRetriesLimit { get; set; }

    public TimeSpan ReconnectionDelay { get; set; } = TimeSpan.FromSeconds(3);
}

