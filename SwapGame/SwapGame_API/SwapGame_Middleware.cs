using System.Diagnostics;

namespace SwapGame_API {
    public struct Header {
        public string key;
        public string value;
        public Header(string header, string value) {
            this.key = header;
            this.value = value;
        }
    }

    public class SwapGame_Middleware {
        private readonly string[] _allowed_origins;

        private readonly Header[] _default_headers;

        private readonly RequestDelegate _next;
        public SwapGame_Middleware(RequestDelegate next, Header[] default_headers, string[] allowed_origins) {
            _next = next;
            _default_headers = default_headers;
            _allowed_origins = allowed_origins;
        }

        public async Task Invoke(HttpContext context) {

            if (_default_headers is not null)
                foreach (var header in _default_headers) {
                    context.Response.Headers[header.key] = header.value;
                }

            string request_origin = context.Request.Headers["Origin"];

            if (Contains(_allowed_origins, request_origin)) {
                AllowOrigin(context, request_origin);
            } else {
                // Messages do not appear in console, rather in Visual Studio's "Output" window
                Debug.WriteLine($"RECEIVED REQUEST FROM DISALLOWED ORIGIN: \"{request_origin}\"");
                DisallowOrigin(context);
            }

            await _next(context);
        }

        private static void AllowOrigin(HttpContext context, string origin) {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        }

        private static void DisallowOrigin(HttpContext context) {
            context.Response.Headers.Remove("Access-Control-Allow-Origin");
        }

        private static bool Contains<T>(T[] arr, T item) => Array.IndexOf(arr, item) != -1;
    }

    static class SwapGame_Middleware_Extensions {
        public static void UseSwapGame_Middleware(this WebApplication app, Header[] default_headers, string[] allowed_origins) {
            app.UseMiddleware<SwapGame_Middleware>(default_headers, allowed_origins);
        }
    }
}
