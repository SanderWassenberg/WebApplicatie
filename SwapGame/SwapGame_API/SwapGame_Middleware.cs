
namespace SwapGame_API; 

public struct Header {
    public string key;
    public string value;
    public Header(string header, string value) {
        this.key = header;
        this.value = value;
    }
}

public class SwapGame_Middleware {

    private readonly Header[] _default_headers;

    private readonly RequestDelegate _next;
    public SwapGame_Middleware(RequestDelegate next, Header[] default_headers) {
        _next = next;
        _default_headers = default_headers;
    }

    public async Task Invoke(HttpContext context) {

        if (_default_headers is not null)
            foreach (var header in _default_headers) {
                context.Response.Headers[header.key] = header.value;
            }

        await _next(context);
    }

    private static bool Contains<T>(T[] arr, T item) => Array.IndexOf(arr, item) != -1;
}

static class SwapGame_Middleware_Extensions {
    public static void UseSwapGame_Middleware(this WebApplication app, Header[] default_headers) {
        app.UseMiddleware<SwapGame_Middleware>(default_headers);
    }
}
