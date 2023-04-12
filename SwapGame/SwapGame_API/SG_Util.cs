using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using SendGrid;
using SwapGame_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SwapGame_API;

public static class SG_Util {
    // Dear microsoft, please stop requiring me to instantiate objects every time I want to perform a simple task. Thanks.
    // Making this static is fine, it's thread safe because it only uses a regex internally. https://learn.microsoft.com/en-us/dotnet/standard/base-types/thread-safety-in-regular-expressions
    private static EmailAddressAttribute email_attrib = new EmailAddressAttribute(); 
    public static bool is_valid_email(string email) => email_attrib.IsValid(email);

    public static async Task<string> build_jwt_token(JwtOptions options, SwapGameUser user, UserManager<SwapGameUser> user_manager) {
        // Voor hulp, zie deze repo: https://github.com/joydipkanjilal/jwt-aspnetcore

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var roles = await user_manager.GetRolesAsync(user);

        for (int i = 0; i < roles.Count; i++) {
            claims.Add(new Claim(ClaimTypes.Role, roles[i]));
        }

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer:   options.Issuer,
            audience: options.Audience,
            claims:   claims,
            expires:  DateTime.Now.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256)
        ));
    }

    public static async Task<bool> send_confirmation_email(string email_address, EmailOptions options) {
        var client = new SendGridClient(options.Api_Key);

        var msg = new SendGridMessage() {
            From = new EmailAddress(options.Sender_Address, "SwapGame"),
            Subject = "Welcome to SwapGame",
            PlainTextContent = "Your account has been successfully made!"
        };

        msg.AddTo(new EmailAddress(email_address));
        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }
}
