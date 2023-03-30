using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SwapGame_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
                    ValidateIssuer = true,
                    ValidIssuer    = issuer,

                    ValidateAudience = true,
                    ValidAudience    = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                    ValidateLifetime = false,
                };
            });
            builder.Services.AddAuthorization();


            var app = builder.Build();

            app.UseExceptionHandler(new ExceptionHandlerOptions {
                ExceptionHandler = async http_context => {
                    await http_context.Response.WriteAsJsonAsync("server broke :(");
                }
            });

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

            app.UseCookiePolicy(new CookiePolicyOptions { 
                HttpOnly = HttpOnlyPolicy.Always,
                Secure   = CookieSecurePolicy.Always
            });

            var api = app.MapGroup("api").RequireCors(MyCorsSettings);
            api.MapGet (nameof(admins_only),   admins_only);
            api.MapGet (nameof(jwt_only),      jwt_only);
            api.MapGet (nameof(exception),     exception);
            api.MapPost(nameof(signup),        signup);
            api.MapPost(nameof(login),         login);

            app.Run();
        }

        [Authorize(Roles = "Administrator")]
        static string admins_only() => "Dit is alleen voor admins";

        [Authorize] 
        static string jwt_only() => "dit kan je alleen zien met een jwt token";

        [AllowAnonymous]
        static string exception() {
            throw new Exception("oepsiepoepsie, in deze exception staat gevoelige informatie owo");
            return "zou je niet moeten zien omdat deze endpoint eerst een exception gooit";
        }

        [AllowAnonymous]
        static async Task<IResult> login(
            LoginData login,
            HttpContext http_context,
            //SwapGame_DbContext db_context,
            //ILoggerFactory lf,
            UserManager<IdentityUser> user_manager,
            ConfigurationManager config)  
        {
            //var logger = lf.CreateLogger("request_token");

            var login_succes = await SG_Util.Login(user_manager, login);

            if (!login_succes) 
                return Results.Unauthorized();

            var stringToken = SG_Util.BuildJwtToken(config["Jwt:Key"], config["Jwt:Issuer"], config["Jwt:Audience"], login.Name);
            http_context.Response.Cookies.Append(
                "SG-api-token",
                stringToken
            );
            //http_context.Session.

            return Results.Ok();
        }

        [AllowAnonymous]
        static async Task<IResult> signup(
            SignupData signup,
            UserManager<IdentityUser> user_manager)
        {
            if (!signup.complete()) return Results.BadRequest("incomplete signup data");

            var identityUser = new IdentityUser(signup.Name);

var result = await user_manager.CreateAsync(identityUser, signup.Password);

            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        }
    }
}