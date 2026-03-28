using System.Collections.Concurrent;

namespace WebMediator;

public sealed class WebMediatorRequest
{
    internal WebMediatorRequest(HttpContext httpContext, Type dataType, Func<Type, Task<object?>> dataGetter)
    {
        HttpContext = httpContext;
        DataType = dataType;
        _dataGetter = dataGetter;
    }

    readonly Func<Type, Task<object?>> _dataGetter;
    readonly ConcurrentDictionary<Type, Task<object?>> _dataTask = [];

    public Type DataType { get; }

    public object CreateInstanceOfDataType() => DataType.CreateInstance()!;

    public Task<object?> ReadData() => ReadDataAs(DataType);

    public async Task<T?> ReadDataAs<T>() => (T?)await ReadDataAs(typeof(T));

    public Task<object?> ReadDataAs(Type resultType)
    {
        if (!_dataTask.IsEmpty && !_dataTask.ContainsKey(resultType))
            throw new InvalidOperationException($"The request data was already read as '{_dataTask.Keys.First()}'.");

        return _dataTask.GetOrAdd(resultType, _dataGetter);
    }

    public HttpContext HttpContext { get; }

    public IServiceProvider Services => this.HttpContext.RequestServices;

    public CancellationToken CancellationToken => this.HttpContext.RequestAborted;
}
