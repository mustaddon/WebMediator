using System.Collections.Concurrent;

namespace WebMediator.Extensions;

public static class TypeExtensions
{
    internal static bool IsAttribute(this Type type) => typeof(Attribute).IsAssignableFrom(type);

    internal static bool IsException(this Type type) => typeof(Exception).IsAssignableFrom(type);

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

    internal static PropertyInfo? GetStreamProperty(this Type type)
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
}
