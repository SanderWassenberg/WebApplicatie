using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SwapGame_API.endpoints;

public struct SignupData {
    public string Name { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    // ! Must update complete() when adding new fields !

    public bool complete() =>
        !string.IsNullOrEmpty(this.Name) &&
        !string.IsNullOrEmpty(this.Password) &&
        !string.IsNullOrEmpty(this.Email);
}

struct SignupResponse {
    public bool sent_email { get; set; }
    public bool for_real { get; set; }

    public IEnumerable<IdentityError> identity_errors { get; set; }
    public string error_message { get; set; }
}

public static partial class Endpoints {
    [AllowAnonymous]
    public static async Task<IResult> signup(
        [FromBody] SignupData signup,
        UserManager<SwapGameUser> user_manager,
        IOptions<EmailOptions> email_options,
        IWebHostEnvironment env) {

        if (!signup.complete()) {
            return Results.BadRequest(new SignupResponse { error_message = "Incomplete signup data, one or more fields missing." });
        }

        if (!SG_Util.is_valid_email(signup.Email)) {
            return Results.BadRequest(new SignupResponse { error_message = "Invalid email." });
        }

        var user = new SwapGameUser(signup.Name) { Email = signup.Email };
        var user_creation_result = await user_manager.CreateAsync(user, signup.Password);

        if (!user_creation_result.Succeeded) {
            return Results.BadRequest(new SignupResponse { identity_errors = user_creation_result.Errors });
        }

        if (env.IsDevelopment()) {
            return Results.Ok(new SignupResponse { sent_email = true }); // just pretend, it's development evnironment we don't care
        }

        return Results.Ok(new SignupResponse {
            sent_email = await SG_Util.send_confirmation_email(user.Email, email_options.Value),
            for_real = true
        });
    }
}