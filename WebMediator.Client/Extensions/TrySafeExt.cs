namespace WebMediator.Client.Extensions;

internal static class TrySafeExt
{
    internal static async Task<TResult?> TrySafe<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> fn, Action<Exception>? onError = null)
    {
        try
        {
            return await fn(source);
        }
        catch(Exception ex)
        {
            onError?.Invoke(ex);
            return default;
        }
    }

}
