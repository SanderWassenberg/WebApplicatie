using Microsoft.AspNetCore.Identity;

namespace SwapGame_API;

public static class SeedData {


    // seeding roles
    // https://stackoverflow.com/questions/34343599/how-to-seed-users-and-roles-with-code-first-migration-using-identity-asp-net-cor
    // getting a service instance
    // https://stackoverflow.com/questions/59774559/how-do-i-get-a-instance-of-a-service-in-asp-net-core-3-1
    public static async Task Initialize(IServiceProvider services) {

        using (var scope = services.CreateScope()) {
            var role_manager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            if (role_manager is null) 
                throw new Exception("Couldn't seed database, unable to get a RoleManager service.");

            for (int i = 0; i < Roles.List.Length; i++) {
                var role = role_manager.Roles.SingleOrDefault(r => r.Name == Roles.List[i]);
                if (role is null) { 
                    await role_manager.CreateAsync(new IdentityRole(Roles.List[i]));
                }
            }

            var user_manager = scope.ServiceProvider.GetService<UserManager<SwapGameUser>>();

            async Task confirm_role(SwapGameUser user, string role) {
                if (!await user_manager.IsInRoleAsync(user, role)) {
                    await user_manager.AddToRoleAsync(user, role);
                }
            }

            const string AdminDefaultPassword = "SuperGeheim1!";

            var admin = user_manager.Users.SingleOrDefault(u => u.UserName == "admin");

            if (admin is null) {
                await user_manager.CreateAsync(new SwapGameUser("admin"), AdminDefaultPassword);
            }

            if (!await user_manager.HasPasswordAsync(admin)) {
                await user_manager.AddPasswordAsync(admin, AdminDefaultPassword);
            }

            await confirm_role(admin, Roles.Admin);
            await confirm_role(admin, Roles.Default);
        }
    }
}
