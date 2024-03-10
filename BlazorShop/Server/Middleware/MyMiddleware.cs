
namespace BlazorShop.Server.Middleware
{
    public class MyMiddleware 
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TO DO: before logic
            await _next(context);
            // TO DO: after logic
        }
    }
}
