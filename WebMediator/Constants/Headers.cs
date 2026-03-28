namespace WebMediator.Constants;

internal static class Headers
{
    public const string DATA = "data";
    public const string DATA_TYPE = "data-type";
    public const string DATA_STREAM_PROPERTY = "data-stream-property";

    public static readonly string CorsExpose = string.Join(",", [
        DATA,
        DATA_TYPE,
        DATA_STREAM_PROPERTY
    ]);
}
