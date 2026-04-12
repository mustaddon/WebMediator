using System.Collections.Concurrent;
using System.Net.ServerSentEvents;

namespace WebMediator.Client.Extensions;

internal static class TypeExtensions
{
    public static bool IsAbstractPlus(this Type type)
    {
        return type.IsAbstract 
            && type != typeof(Stream) 
            && type != typeof(Array)
            && !type.IsIAsyncEnumerable();
    }

    public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

    public static bool IsAttribute(this Type type) => typeof(Attribute).IsAssignableFrom(type);

    public static bool IsException(this Type type) => typeof(Exception).IsAssignableFrom(type);

    public static PropertyInfo? GetStreamProperty(this Type type)
    {
        if (type.IsAbstract)
            return null;

        return _typeStreamProps.GetOrAdd(type, k => type
            .GetProperties()
            .FirstOrDefault(x => x.CanRead
                && x.CanWrite
                && x.SetMethod?.IsStatic == false
                && x.PropertyType == typeof(Stream)));
    }

    static readonly ConcurrentDictionary<Type, PropertyInfo?> _typeStreamProps = [];

    internal static bool IsIAsyncEnumerable(this Type type)
        => type.IsInterface
        && type.IsGenericType
        && !type.IsGenericTypeDefinition
        && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);

    internal static Type? GetAsyncEnumerableItemType(this Type type)
    {
        if (type.IsIAsyncEnumerable())
            return type.GetGenericArguments().FirstOrDefault();

        return type.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && !type.IsGenericTypeDefinition
                && x.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            ?.GetGenericArguments().FirstOrDefault();
    }

    internal static bool TryGetSseItemType(this Type type, out Type itemType)
    {
        if (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(SseItem<>))
        {
            itemType = type.GetGenericArguments().FirstOrDefault()!;
            return true;
        }

        itemType = default!;
        return false;
    }
}
