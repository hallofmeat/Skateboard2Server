using Microsoft.AspNetCore.Connections;

namespace Skateboard2Server.Host
{
    public static class ConnectionBuilderExtensions
    {
        public static TBuilder UseSslServer<TBuilder>(this TBuilder builder) where TBuilder : IConnectionBuilder
        {
            builder.Use(next =>
            {
                var middleware = new SslServerMiddleware(next);
                return middleware.OnConnectionAsync;
            });
            return builder;
        }
    }
}