using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace WebMediator.Extensions;

internal static class IAsyncEnumerableExt
{
    internal static async IAsyncEnumerable<SseItem<string>> ToSse(this IAsyncEnumerable<object?> source,
        JsonSerializerOptions options,
        Type itemType,
        bool isSseItemType,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (itemType == typeof(object))
        {
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

            if (!await enumerator.MoveNextAsync())
                yield break;

            if (enumerator.Current?.GetType().TryGetSseItemType(out _) == true)
            {
                do
                {
                    yield return ToSseString(enumerator.Current, options);
                }
                while (await enumerator.MoveNextAsync());
            }
            else
            {
                do
                {
                    yield return new(JsonSerializer.Serialize(enumerator.Current, options));
                }
                while (await enumerator.MoveNextAsync());
            }
        }
        else if (isSseItemType)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
                if (item != null)
                    yield return ToSseString(item, options);
        }
        else
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
                yield return new(JsonSerializer.Serialize(item, options));
        }
    }

    static SseItem<string> ToSseString(object sse, JsonSerializerOptions options)
    {
        var type = sse.GetType();

        var data = type.GetProperty(nameof(SseItem<>.Data))?.GetValue(sse);
        var eventType = type.GetProperty(nameof(SseItem<>.EventType))?.GetValue(sse) as string;
        var eventId = type.GetProperty(nameof(SseItem<>.EventId))?.GetValue(sse) as string;

        return new(JsonSerializer.Serialize(data, options), eventType)
        {
            EventId = eventId
        };
    }

    internal static IAsyncEnumerable<object?> ToAsyncEnumerable(this object source, Type itemType, CancellationToken cancellationToken)
    {
        if (source is IAsyncEnumerable<object?> asyncObj)
            return asyncObj;

        return (IAsyncEnumerable<object?>)_castMethod.MakeGenericMethod(itemType).Invoke(null, [source, cancellationToken])!;
    }

    static async IAsyncEnumerable<object?> CastAdv<T>(this IAsyncEnumerable<T> source,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
            yield return item;
    }

    static readonly MethodInfo _castMethod = new Func<IAsyncEnumerable<object>, CancellationToken, IAsyncEnumerable<object?>>(CastAdv<object>).Method.GetGenericMethodDefinition();


}
