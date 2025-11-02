using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Repositries
{
    public class ProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAll() => await _context.Products.ToListAsync();
        public async Task<Product?> Get(int id) => await _context.Products.FindAsync(id);
        public async Task Add(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task Update(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
