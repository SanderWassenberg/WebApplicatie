using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SwapGame_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SwapGame_API {
    public static class SG_Util {

        public static async Task<bool> Login(UserManager<IdentityUser> user_manager, LoginData login) {
            var identity_user = await user_manager.FindByNameAsync(login.Name);
            return await user_manager.CheckPasswordAsync(identity_user, login.Password);
        }

        // cring bs why do I need to instantiate a class fuck off with that
        private static JwtSecurityTokenHandler jwt_handler = new JwtSecurityTokenHandler();

        public static string BuildJwtToken(string key, string issuer, string audience, string username) {
            // Voor hulp, zie deze repo: https://github.com/joydipkanjilal/jwt-aspnetcore

            return jwt_handler.WriteToken(new JwtSecurityToken(
                issuer:   issuer,
                audience: audience,
                claims: new[] {
                    new Claim(ClaimTypes.Name, username),
                    // This GUID makes the token unique, otherwise it we would generate the same token every time
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                },
                expires:  DateTime.Now.AddMinutes(30),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
            ));
        }
    }
}
