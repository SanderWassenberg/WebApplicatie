using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SwapGame_API; 

// Powershell variant
// > add-migration name_of_migration -Context SwapGame_IdentityDbContext
// > update-database -Context SwapGame_IdentityDbContext

// dotnet ef tool variant. (check eerst of deze is geinstalleerd, instructies verder hieronder)
// > dotnet-ef migrations add SwapGame_IdentityDbContext --context SwapGame_IdentityDbContext --project SwapGame_API
// > dotnet-ef database update --context SwapGame_IdentityDbContext --project SwapGame_API

// Hoe te zien of dotnet-ef is geinstalleerd:
// > dotnet tool list --global
// Om te installeren:
// > dotnet tool install --global dotnet-ef

public class SwapGame_IdentityDbContext : IdentityDbContext<SwapGameUser, IdentityRole, string> {
    public SwapGame_IdentityDbContext(DbContextOptions<SwapGame_IdentityDbContext> options) : base(options) { }

    //DbSet<Models.Test> Tests { get; set; }

    protected override void OnModelCreating(ModelBuilder model_builder) {
        base.OnModelCreating(model_builder);

    }
}
