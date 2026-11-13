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
    readonly object _dataSync = new();
    Type? _dataResultType;
    Task<object?>? _dataTask;

    public Type DataType { get; }

    public object CreateInstanceOfDataType() => DataType.CreateInstance()!;

    public Task<object?> ReadData() => ReadDataAs(DataType);

    public async Task<T?> ReadDataAs<T>() => (T?)await ReadDataAs(typeof(T));

    public Task<object?> ReadDataAs(Type resultType)
    {
        lock (_dataSync)
        {
            if (_dataTask != null)
            {
                if (_dataResultType != resultType)
                    throw new InvalidOperationException($"The request data was already read as '{_dataResultType}'.");

                return _dataTask;
            }

            _dataResultType = resultType;
            _dataTask = _dataGetter(resultType);
            return _dataTask;
        }
    }

    public HttpContext HttpContext { get; }

    public IServiceProvider Services => this.HttpContext.RequestServices;

    public CancellationToken CancellationToken => this.HttpContext.RequestAborted;
}
