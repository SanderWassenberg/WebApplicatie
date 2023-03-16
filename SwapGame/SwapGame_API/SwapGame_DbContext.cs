using Microsoft.EntityFrameworkCore;

namespace SwapGame_API {
    public class SwapGame_DbContext : DbContext {
        public SwapGame_DbContext(DbContextOptions<SwapGame_DbContext> options) : base(options) { }

        //public DbSet<Thing> Things { get; set; }
    }
}
