using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SwapGame_API.endpoints;

public struct LoginData {
    public string Name { get; set; }
    public string Password { get; set; }


}

public struct LoginResponse {
    public string jwt_token { get; set; }
    public string error_message { get; set; }
}

public static partial class Endpoints {
    [AllowAnonymous]
    public static async Task<IResult> login(
        [FromBody] LoginData login
        , HttpContext http_context
        //, SwapGame_DbContext db_context
        //, ILoggerFactory lf
        , UserManager<SwapGameUser> user_manager
        , SignInManager<SwapGameUser> signin_manager
        , IOptions<JwtOptions> jwt_options
    ) {
        //var logger = lf.CreateLogger("request_token");

        var user = signin_manager.UserManager.Users.SingleOrDefault(u => u.UserName == login.Name);
        if (user is null)
            return Results.BadRequest(new LoginResponse { error_message = "No such user." });

        var r = await signin_manager.CheckPasswordSignInAsync(user, login.Password, false);

        if (!r.Succeeded)
            return Results.BadRequest(new LoginResponse { error_message = "Incorrect password" });

        return Results.Ok(new LoginResponse {
            jwt_token = await SG_Util.build_jwt_token(jwt_options.Value, user, signin_manager.UserManager)
        });
    }
}