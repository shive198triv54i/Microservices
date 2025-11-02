using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService
{
    public class ProductDbContext: DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        { }

        public DbSet<Product> Products { get; set; }
    }
}
