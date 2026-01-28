using Microsoft.EntityFrameworkCore;

namespace TokenManagerApi.Models
{
    public class TokenManagerDbContext : DbContext
    {
        public TokenManagerDbContext(DbContextOptions<TokenManagerDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}