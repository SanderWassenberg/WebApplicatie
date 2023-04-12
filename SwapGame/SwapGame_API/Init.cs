using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SwapGame_API;

public static class Init {

    public static string MyCorsSettings = "just_work_ffs";

    public struct Init_Builder_Result {
        public bool has_connection_string { get; set; }
        public bool configured_properly { get; set; }
    }
    public static Init_Builder_Result init_builder(WebApplicationBuilder builder) {

        var r = new Init_Builder_Result();

        // Ik zou een early return willen doen hier maar als ik dat doe dan werken de update-database dingen niet meer blijkbaar,
        // want om een of andere reden voeren die gewoon dit hele programma uit ofzo, echt door idioten bedacht.
        r.configured_properly = verify_configuration(builder.Configuration);

        // Gekut met de database
        {
            var connection_string = builder.Configuration["SwapGame:Extreem_Veilige_DB_pls_no_hack"];
            r.has_connection_string = connection_string != "<MISSING>";

            if (!r.has_connection_string)
                return r;

            //builder.Services.AddDbContext<SwapGame_DbContext>(
            //    o => o.UseSqlServer(connection_string));

            builder.Services.AddDbContext<SwapGame_IdentityDbContext>(
                o => o.UseSqlServer(connection_string));
        }

        // Gekut met CORS:
        // https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-7.0#enable-cors-with-endpoint-routing
        builder.Services.AddCors(options => {
            options.AddPolicy(MyCorsSettings, policy => {
                if (builder.Environment.IsDevelopment()) {
                    policy.WithOrigins("https://localhost:7036");
                    policy.WithOrigins("http://127.0.0.1:5500");
                }
                policy.WithOrigins("https://sanderwassenberg.hbo-ict.org");
                policy.WithHeaders("Content-Type"); // allowed headers.
            });
        });

        builder.Services.AddIdentity<SwapGameUser, IdentityRole>()
            .AddEntityFrameworkStores<SwapGame_IdentityDbContext>()
            .AddDefaultTokenProviders();

        // TODO: Volg deze 2 tutorials: https://stackoverflow.com/questions/72350711/how-do-i-add-authentication-to-an-asp-net-core-minimal-api-using-identity-but-w
        builder.Services.AddAuthentication(o => {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o => {
            var issuer = builder.Configuration["SwapGame:Jwt:Issuer"];
            var audience = builder.Configuration["SwapGame:Jwt:Audience"];
            var key = builder.Configuration["SwapGame:Jwt:Key"];

            o.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                ValidateLifetime = false,
            };
        });
        builder.Services.AddAuthorization();


        builder.Services.Configure<SwapGameOptions>(
            builder.Configuration.GetSection("SwapGame"));
        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("SwapGame:Email"));
        builder.Services.Configure<JwtOptions>(
            builder.Configuration.GetSection("SwapGame:Jwt"));

        r.has_connection_string = true;
        return r;
    }

    public static async Task init_app(WebApplication app) {

        bool is_development = app.Environment.IsDevelopment();
        app.Logger.LogInformation("is development: {}", is_development);

        await SeedData.Initialize(app.Services);

        if (!is_development) {
            app.UseExceptionHandler(new ExceptionHandlerOptions {
                ExceptionHandler = async http_context => {
                    await http_context.Response.WriteAsJsonAsync("server broke :(");
                }
            });
        }

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
            Secure = CookieSecurePolicy.Always
        });
    }


    static bool verify_configuration(ConfigurationManager config) {

        var sg = new SwapGameOptions(); config.GetSection(sg.Path).Bind(sg);
        var em = new EmailOptions();    config.GetSection(em.Path).Bind(em);
        var jw = new JwtOptions();      config.GetSection(jw.Path).Bind(jw);

        var missing_config_values = new List<string>();

        void check(string value, string location) {
            if (string.IsNullOrEmpty(value))
                missing_config_values.Add(location);
        }

        check(sg.Extreem_Veilige_DB_pls_no_hack, sg.Path + ":" + nameof(sg.Extreem_Veilige_DB_pls_no_hack));
        check(em.Api_Key,                        em.Path + ":" + nameof(em.Api_Key));
        check(em.Sender_Address,                 em.Path + ":" + nameof(em.Sender_Address));
        check(jw.Key,                            jw.Path + ":" + nameof(jw.Key));

        if (missing_config_values.Count == 0) 
            return true;

        Console.Error.WriteLine("Please fill out the following values in appsettings.json:");

        foreach (var missing_value in missing_config_values)
            Console.Error.WriteLine("* {0}", missing_value);
        
        return false;
    }

    
}
