using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SwapGame_API.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using SwapGame_API.endpoints;

namespace SwapGame_API;

public class Program {


    public static async Task Main(string[] args) {

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
        
        await Init.init_app(app);

        var api = app.MapGroup("api").RequireCors(Init.MyCorsSettings);
        api.MapGet(nameof(admins_only), admins_only);
        api.MapGet(nameof(list_users), list_users);
        api.MapGet(nameof(jwt_only), jwt_only);
        api.MapGet(nameof(exception), exception);
        api.MapGet(nameof(test), test);
        api.MapPost(nameof(ping), ping);
        api.MapPost(nameof(Endpoints.signup), Endpoints.signup);
        api.MapPost(nameof(Endpoints.login), Endpoints.login);

        app.Run();
    }


    [AllowAnonymous]
    static IResult test(
        UserManager<SwapGameUser> user_manager,
        RoleManager<IdentityRole> role_manager,
        IOptions<SwapGameOptions> sg_iopt
    ) {
        return Results.Ok();
    }


    [Authorize(Roles = Roles.Admin)]
    static IResult list_users(SwapGame_IdentityDbContext ctx) {
        var list = ctx.Users.Select(u => new UserData {
            name = u.UserName,
            email = u.Email,
            roles = 
                ctx.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ctx.Roles
                        .Single(r => r.Id == ur.RoleId)
                        .Name)
                    .AsEnumerable()
        });

        return Results.Ok(list);
    
    }

    [Authorize(Roles = Roles.Admin)]
    static string admins_only() => "Dit is alleen voor admins";

    [Authorize(Roles = Roles.Default)]
    static string jwt_only() => "dit kan je alleen zien met een jwt token";

    [AllowAnonymous] static string exception() {
        throw new Exception("oepsiepoepsie, in deze exception staat gevoelige informatie owo");
    }

    [AllowAnonymous] static IResult ping() => Results.Ok();
}