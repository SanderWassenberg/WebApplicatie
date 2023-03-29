using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SwapGame_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SwapGame_API
{
    public class Program {

        static string MyCorsSettings = "just_work_ffs";

        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Gekut met de database
            var connection_string = builder.Configuration.GetConnectionString("Extreem_Veilige_DB_pls_no_hack");

            builder.Services.AddDbContext<SwapGame_DbContext>(
                o => o.UseSqlServer(connection_string));

            builder.Services.AddDbContext<SwapGame_IdentityDbContext>(
                o => o.UseSqlServer(connection_string));

            // Mijn eigen gerotzooi om er maar voor te zorgen dat ik in de functies toegang heb tot de Configuration.
            // Dit zorgt ervoor dat ik in de MapWhatever functies een callback kan gooien die een ConfigurationManager verwacht
            // en dan wordt die magisch met dependency injection ofzo meegegeven.
            builder.Services.Add(new ServiceDescriptor(
                typeof(ConfigurationManager), 
                sp => builder.Configuration, 
                ServiceLifetime.Singleton)
            );

            // Gekut met CORS:
            // https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-7.0#enable-cors-with-endpoint-routing
            builder.Services.AddCors(options => {
                options.AddPolicy(MyCorsSettings, policy => {
                    policy.WithOrigins("https://localhost:7036"); // allowed origins
                    policy.WithHeaders("Content-Type"); // allowed headers.
                });
            });

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<SwapGame_IdentityDbContext>()
                .AddDefaultTokenProviders();

            // TODO: Volg deze 2 tutorials: https://stackoverflow.com/questions/72350711/how-do-i-add-authentication-to-an-asp-net-core-minimal-api-using-identity-but-w
            builder.Services.AddAuthentication(o => {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o => {
                var issuer   = builder.Configuration["Jwt:Issuer"];
                var audience = builder.Configuration["Jwt:Audience"];
                var key      = builder.Configuration["Jwt:Key"];

                o.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = false,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer   = issuer,
                    ValidAudience = audience,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
            builder.Services.AddAuthorization();


            var app = builder.Build();

            app.UseHttpsRedirection();
            
            app.UseCors();

            // these must be in this order!
            //app.UseRouting();        // 1
            //app.UseAuthentication(); // 2
            //app.UseAuthorization();  // 3

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
            app.MapGet("api/weatherforecast", 
                WeatherForecast.Get
            ).RequireCors(MyCorsSettings);

            app.MapPost("api/test", (HttpContext context, Thingy t, ILoggerFactory loggerFactory) => {
                var logger = loggerFactory.CreateLogger("api/test");

                logger.LogInformation("thingy: {}, {}, {}", t.Name, t.Num, t.PropDieIkNietMeegeef);
            }).RequireCors(MyCorsSettings);

            app.MapGet("api/only_for_admins", [Authorize(Roles = "Administrator")] (SwapGame_DbContext context, HttpContext http_context) => {
                return "Dit is alleen voor admins";
            }).RequireCors(MyCorsSettings);
            app.MapGet("api/only_with_jwt", [Authorize] (SwapGame_DbContext context, HttpContext http_context) => {
                return "dit kan je alleen zien met een jwt token";
            }).RequireCors(MyCorsSettings);

            app.MapPost("api/request_token", [AllowAnonymous] async (LoginData login,
            HttpContext http_context,
            SwapGame_DbContext db_context,
            //ILoggerFactory lf,
            UserManager<IdentityUser> user_manager,
            ConfigurationManager config) => {
                //var logger = lf.CreateLogger("request_token");

                var login_succes = await SG_Util.Login(user_manager, login);

                if (!login_succes) 
                    return Results.Unauthorized();

                var stringToken = SG_Util.BuildJwtToken(config["Jwt:Key"], config["Jwt:Issuer"], config["Jwt:Audience"], login.Name);

                return Results.Ok(stringToken);
            }).RequireCors(MyCorsSettings); ;

            app.MapGet("api/hash_password", (string pw) => {
                return SG_Util.HashPassword(pw);
            }).RequireCors(MyCorsSettings);

            app.MapGet("api/verify_password", (string hash, string pw, ILoggerFactory f) => {
                var l = f.CreateLogger("verify password"); // cringe factory pattern
                l.LogInformation("the hash is `{}`", hash);
                return SG_Util.VerifyPassword(pw, hash) ? "true" : "false";
            }).RequireCors(MyCorsSettings);

            app.MapPost("api/signup", [AllowAnonymous] async (
                SignupData signup, 
                UserManager<IdentityUser> user_manager) => {
                var identityUser = new IdentityUser(signup.Name) {
                    //Email = signup.Name + "@fake_email.com" // stupid bs Identity crap requires you to fill in an email suck my nuts
                };

                var result = await user_manager.CreateAsync(identityUser, signup.Password);

                return result.Succeeded
                    ? Results.Ok()
                    : Results.BadRequest(result.Errors);
            })
                .RequireCors(MyCorsSettings);
            ;

            app.Run();
        }
    }



    class Thingy {
        public string? Name { get; set; }
        public int? Num { get; set; }
        public int? PropDieIkNietMeegeef { get; set; }
    }
}