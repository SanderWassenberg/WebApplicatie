using Microsoft.EntityFrameworkCore;
using SwapGame_API.Models;

namespace SwapGame_API {
    public class SwapGame_DbContext : DbContext {
        public SwapGame_DbContext(DbContextOptions<SwapGame_DbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder model_builder) {
            model_builder.Entity<User>().HasData(
                new User() {
                    Id = 1,
                    Name = "allereerste_gebruiker",
                }
            );
        }
    }
}
