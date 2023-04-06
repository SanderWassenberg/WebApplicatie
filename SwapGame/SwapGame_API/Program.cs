using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SwapGame_API.Models;
using System.Text;

using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace SwapGame_API
{
    public class Program {


        public static void Main(string[] args) {

            WebApplication app;

            {
                var builder = WebApplication.CreateBuilder(args);

                var r = Init.init_builder(builder);

                if (!r.has_connection_string) {
                    Console.Error.WriteLine("Oh nyo we kunnen niet vewdew zondew connection stwing uwuw >:(\nGa die maar even gauw invullen jij imbiciel.");
                    return;
                }

                app = builder.Build();

                // Dit moeten we na builder.Build doen omdat anders update-database commands niet meer goed werken, super scheef.
                // Op het moment dat update database gedoe wordt gedaan dan wordt alles tot aan builder.Build uitgevoerd en
                // daarna doet hij zijn eigen ding, deze code wordt dan nooit bereikt. Als de API normaal uitgevoerd wordt dus wel,
                // en dan willen we die early return uitvoeren.
                if (!r.configured_properly) {
                    Console.Error.WriteLine("Missing items in configuration, exiting...");
                    return;
                }
            }
            
            Init.init_app(app);

            var api = app.MapGroup("api").RequireCors(Init.MyCorsSettings);
            api.MapGet(nameof(admins_only), admins_only);
            api.MapGet(nameof(jwt_only), jwt_only);
            api.MapGet(nameof(exception), exception);
            api.MapGet(nameof(test), test);
            api.MapPost(nameof(signup), signup);
            api.MapPost(nameof(login), login);
            api.MapPost(nameof(email), email);

            app.Run();
        }

        [AllowAnonymous]
        static IResult test(
            UserManager<SwapGameUser> user_manager,
            RoleManager<IdentityRole> role_manager,
            IOptions<SwapGameOptions> sg_iopt
        ) {
            var opt = sg_iopt.Value;
            return Results.Ok(opt);
        }

        [AllowAnonymous]
        static async Task<IResult> email([FromBody] string email_address, IOptions<EmailOptions> options) {
            var api_key = options.Value.Api_Key;
            var client = new SendGridClient(api_key);
            var msg = new SendGridMessage() {
                // this email must be the one verified at SendGrid (which is my school email)
                From = new EmailAddress(options.Value.Sender_Address, "SwapGame"),
                Subject = "Sending with Twilio SendGrid is Fun",
                PlainTextContent = "and easy to do anywhere, especially with C#"
            };
            msg.AddTo(new EmailAddress(email_address, "NAAM_VAN_ONTVANGER"));
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
                return Results.Ok("Email queued successfully!");
            else
                return Results.StatusCode(500);
        }

        [Authorize(Roles = Roles.Admin)]
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
            [FromBody] LoginData login
            ,HttpContext http_context
            //,SwapGame_DbContext db_context
            //,ILoggerFactory lf
            //,UserManager<SwapGameUser> user_manager
            ,SignInManager<SwapGameUser> signin_manager
            //,IOptions<JwtOptions> iopts
        ) {
            //var opt = iopts.Value;
            //var logger = lf.CreateLogger("request_token");

            var user = signin_manager.UserManager.Users.SingleOrDefault(u => u.UserName == login.Name);
            if (user is null) {
                return Results.BadRequest("no such user");
            }

            var r = await signin_manager.CheckPasswordSignInAsync(user, login.Password, false);

            if (!r.Succeeded) 
                return Results.Unauthorized();

            //var stringToken = SG_Util.BuildJwtToken(opt.Key, opt.Issuer, opt.Audience, login.Name);
            //http_context.Response.Cookies.Append(
            //    "SG-api-token",
            //    stringToken
            //);
            //http_context.Session.

            return Results.Ok();
        }

        [AllowAnonymous]
        static async Task<IResult> signup(
            [FromBody] SignupData signup,
            UserManager<SwapGameUser> user_manager)
        {
            if (!signup.complete()) return Results.BadRequest("incomplete signup data");

            var user = new SwapGameUser(signup.Name);

var result = await user_manager.CreateAsync(user, signup.Password);

            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        }


    }
}