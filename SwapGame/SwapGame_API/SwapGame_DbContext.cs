using Microsoft.EntityFrameworkCore;
using SwapGame_API.Models;

namespace SwapGame_API {
    public class SwapGame_DbContext : DbContext {
        public SwapGame_DbContext(DbContextOptions<SwapGame_DbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
