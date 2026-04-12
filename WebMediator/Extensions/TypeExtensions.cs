using System.Net.ServerSentEvents;

namespace WebMediator.Extensions;

public static class TypeExtensions
{
    public static object? CreateInstance(this Type type)
    {
        if (type.IsAbstract)
            throw new InvalidOperationException($"Cannot create an instance of an abstract type '{type.Name}'.");

        if (type.IsArray)
            return Array.CreateInstance(type.GetElementType()!, 0);

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        var ctorParams = type.GetConstructors()
            .Select(x => x.GetParameters())
            .MinBy(c => c.Length);

        return Activator.CreateInstance(type, new object?[ctorParams?.Length ?? 0]);
    }

    internal static bool IsAttribute(this Type type) => typeof(Attribute).IsAssignableFrom(type);

    internal static bool IsException(this Type type) => typeof(Exception).IsAssignableFrom(type);

    internal static Type ToAsyncEnumerableType(this Type type)
    {
        return typeof(IAsyncEnumerable<>).MakeGenericType(type);
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

    internal static Type? GetAsyncEnumerableType(this Type type)
    {
        var item = type.GetAsyncEnumerableItemType();
        return item == null ? null : typeof(IAsyncEnumerable<>).MakeGenericType(item);
    }
}
