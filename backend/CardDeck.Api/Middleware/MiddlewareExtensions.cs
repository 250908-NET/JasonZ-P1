namespace CardDeck.Api.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Registration method for the global exception handler middleware.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
