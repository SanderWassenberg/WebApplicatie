using Microsoft.AspNetCore.Identity;

namespace SwapGame_API;

public static class SeedData {


    // seeding roles
    // https://stackoverflow.com/questions/34343599/how-to-seed-users-and-roles-with-code-first-migration-using-identity-asp-net-cor
    // getting a service instance
    // https://stackoverflow.com/questions/59774559/how-do-i-get-a-instance-of-a-service-in-asp-net-core-3-1
    public static async Task Initialize(IServiceProvider services) {

        using var scope = services.CreateScope();

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

        if (user_manager is null) 
            throw new Exception("Couldn't get user manager to seed the database");

        async Task confirm_user(string name, string password, params string[] roles) {

            var user = user_manager.Users.SingleOrDefault(u => u.UserName == name);

            if (user is null) {
                var r = await user_manager.CreateAsync(new SwapGameUser(name), password);
                user = user_manager.Users.SingleOrDefault(u => u.UserName == name);
                if (user is null) 
                    throw new Exception("Failed to make a new user");
            }

            // Do not overwrite the password if it has been changed, only add it if it isn't there.
            if (!await user_manager.HasPasswordAsync(user)) {
                await user_manager.AddPasswordAsync(user, password);
            }

            // Assign roles if needed
            for (int i = 0; i < roles.Length; i++) {
                if (!await user_manager.IsInRoleAsync(user, roles[i])) {
                    await user_manager.AddToRoleAsync(user, roles[i]);
                }
            }
        }

        await confirm_user("admin", "SuperGeheim1!", Roles.Admin, Roles.Default);
        await confirm_user("default", "Default1!", Roles.Default);
        await confirm_user("no_roles", "NoRoles1!");
        // There's loads of await stuff goin on here, but don't try to get cute with it and make these run concurrently.
        // The DbContext can only handle one thing happening at a time, so grouping these in a WhenAll or WaitAll will throw an
        // Exception. For that to work we'd need to have separate contexts, each in their own service scope.
        // https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues
    }
}
