using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
    }
}
