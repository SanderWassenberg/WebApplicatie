using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SwapGame_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SwapGame_API;

public static class SG_Util {

    // cring bs why do I need to instantiate a class fuck off with that
    private static JwtSecurityTokenHandler jwt_handler = new JwtSecurityTokenHandler();

    public static async Task<string> BuildJwtToken(JwtOptions options, SwapGameUser user, UserManager<SwapGameUser> user_manager) {
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

        return jwt_handler.WriteToken(new JwtSecurityToken(
            issuer:   options.Issuer,
            audience: options.Audience,
            claims:   claims,
            expires:  DateTime.Now.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256)
        ));
    }
}
