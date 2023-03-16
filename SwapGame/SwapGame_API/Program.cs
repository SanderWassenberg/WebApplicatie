using Microsoft.EntityFrameworkCore;

namespace SwapGame_API
{
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            {
                var connection_string = builder.Configuration.GetConnectionString("Extreem_Veilige_DB_pls_no_hack");
                builder.Services.AddDbContext<SwapGame_DbContext>(options => options.UseSqlServer(connection_string));
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.


            // Order of 'Use' calls: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-7.0&tabs=visual-studio
            // 1. UseRouting,
            // 2. UseAuthentication,
            // 3. UseAuthorization

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseSwapGame_Middleware(
                default_headers: new Header[] {
                    new Header("X-Content-Type-Options",    "nosniff"),
                    new Header("Strict-Transport-Security", "max-age=15724800; includeSubdomains"),
                    new Header("Content-Security-Policy",   "frame-ancestors 'none'"),
                    new Header("X-Frame-Options",           "DENY"),
                },
                allowed_origins: new string[] {
                    "", // when a browser opens the url directly
                    "https://localhost:7036",
                }
            );
                

            app.MapGet("api/weatherforecast", WeatherForecast.Get);

            app.Run();
        }
    }
}