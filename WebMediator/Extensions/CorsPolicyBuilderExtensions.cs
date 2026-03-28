using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Microsoft.AspNetCore.Builder;

public static class WebMediatorCorsExtensions
{
    public static CorsPolicyBuilder AllowMediatorHeaders(this CorsPolicyBuilder builder)
    {
        return builder.WithExposedHeaders(
            Headers.DATA, 
            Headers.DATA_TYPE, 
            Headers.DATA_STREAM_PROPERTY
        );
    }
}
