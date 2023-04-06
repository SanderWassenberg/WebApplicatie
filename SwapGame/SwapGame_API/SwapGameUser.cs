using Microsoft.AspNetCore.Identity;

namespace SwapGame_API;

public class SwapGameUser : IdentityUser {
    public SwapGameUser() : base() {}
    public SwapGameUser(string name) : base(name) {}
}
