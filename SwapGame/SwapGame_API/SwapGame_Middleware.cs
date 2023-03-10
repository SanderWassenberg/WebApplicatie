namespace SwapGame_API
{
    public class SwapGame_Middleware
    {
        private readonly RequestDelegate _next;
        private readonly (string, string)[] _default_headers;
        //private readonly string[] _forbidden_headers;

        public SwapGame_Middleware(RequestDelegate next, (string, string)[] default_headers
            //, string[] forbidden_headers
            )
        {
            _next = next;
            _default_headers   = default_headers;
            //_forbidden_headers = forbidden_headers;
        }

        public async Task Invoke(HttpContext context)
        {
            var response_headers = context.Response.Headers;

            if (_default_headers is not null)
                foreach (var (header, value) in _default_headers)
                    response_headers[header] = value;

            //if (_forbidden_headers is not null)
            //    foreach (var header in _forbidden_headers)
            //        response_headers.Remove(header);

            await _next(context);
        }
    }
}
