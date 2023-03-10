namespace SwapGame_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseMiddleware<SwapGame_Middleware>(
                new[] {
                    ("Access-Control-Allow-Origin", "*")
                }
                //, new string[] { }
            );


            app.MapGet("/weatherforecast", WeatherForecast.Get);

            app.Run();
        }
    }
}