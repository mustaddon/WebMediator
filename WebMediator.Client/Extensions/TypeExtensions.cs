using System.Collections.Concurrent;

namespace WebMediator.Client.Extensions;

internal static class TypeExtensions
{
    public static bool IsAbstractPlus(this Type type) => type.IsAbstract && type != typeof(Stream) && type != typeof(Array);

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
}
