using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace SwapGame_API {
    public static class SG_Util {
        // cringe Microsoft stupid bs why tf does this need to be a generic class implementing an interface when it could have just been two static methods
        private static PasswordHasher<object> hasher = new PasswordHasher<object>();

        public static string HashPassword(string pw) =>
            hasher.HashPassword(null, pw);
        
        public static bool VerifyPassword(string pw, string hash) =>
            hasher.VerifyHashedPassword(null, hash, pw) == PasswordVerificationResult.Success;

        // cring bs why do I need to instantiate a class fuck off with that
        private static JwtSecurityTokenHandler jwt_handler = new JwtSecurityTokenHandler();

        public static string JwtTokenToString(JwtSecurityToken token) => 
            jwt_handler.WriteToken(token);


    }
}
