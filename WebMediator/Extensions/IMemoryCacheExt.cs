using Microsoft.Extensions.Caching.Memory;

namespace WebMediator.Extensions;

internal static class IMemoryCacheExt
{
    static readonly MemoryCacheEntryOptions _entryOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public static PropertyInfo? GetStreamProperty(this IMemoryCache cache, Type type)
    {
        if (type.IsAbstract)
            return null;

        return cache.GetOrCreate(new { TypeStreamProperty = type },

            entry => type
                .GetProperties()
                .FirstOrDefault(x => x.CanRead
                    && x.CanWrite
                    && x.SetMethod?.IsStatic == false
                    && x.PropertyType == typeof(Stream)),

            _entryOptions);
    }

    public static bool TryGetAsyncEnumerableItemType(this IMemoryCache cache, Type sourceType, out Type itemType, out Type? sseType)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        (itemType, sseType) = cache.GetOrCreate(new { AsyncEnumerableSourceType = sourceType },

            entry =>
            {
                var type = sourceType.GetAsyncEnumerableItemType();

                if (type != null && type.TryGetSseItemType(out var sse))
                    return (type, sse);

                return (type, (Type?)null);
            },

            _entryOptions);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        return itemType != null;
    }

}
