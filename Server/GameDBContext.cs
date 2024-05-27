using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace Server;

public class GameDBContext : DbContext
{
    public GameDBContext(DbContextOptions<GameDBContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Hero> Heroes { get; set; }

}
