using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;

namespace SwapGame_API
{
    public class Program {

        static string MyCorsSettings = "just_work_ffs";

        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            var connection_string = builder.Configuration.GetConnectionString("Extreem_Veilige_DB_pls_no_hack");
            builder.Services.AddDbContext<SwapGame_DbContext>(options => { 
                options.UseSqlServer(connection_string);
            });


            // Gekut met CORS:
            // https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-7.0#enable-cors-with-endpoint-routing
            builder.Services.AddCors(options => {
                options.AddPolicy(MyCorsSettings, policy => {
                    policy.WithOrigins("https://localhost:7036"); // allowed origins
                    policy.WithHeaders("Content-Type"); // allowed headers.
                });
            });

            var app = builder.Build();
            app.UseCors();

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
                }
            );
            
            // De functie die je naar MapGet of MapPost etc. paast kan zelf aangeven wat hij wil hebben van de dependency injection.
            // Stop een SwapGame_DbContext in zijn paramaterlijst, boem database access.
            // Stop een WhateverModel in zijn parameters, dan probeert hij de Post request body of Get query args om te zetten naar zo'n object. 
            app.MapGet("api/weatherforecast", WeatherForecast.Get)
                .RequireCors(MyCorsSettings);

            app.MapPost("api/test", (HttpContext context, Thingy t, ILoggerFactory loggerFactory) => {
                var logger = loggerFactory.CreateLogger("api/test");

                logger.LogInformation("thingy: {}, {}, {}", t.Name, t.Num, t.PropDieIkNietMeegeef);
            }).RequireCors(MyCorsSettings);

            app.MapGet("api/list_users", (SwapGame_DbContext context) => {
                return context.Users.AsEnumerable();
            }).RequireCors(MyCorsSettings);

            app.Run();
        }
    }

    class Thingy {
        public string? Name { get; set; }
        public int? Num { get; set; }
        public int? PropDieIkNietMeegeef { get; set; }
    }
}