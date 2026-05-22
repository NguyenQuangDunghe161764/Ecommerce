using Microsoft.EntityFrameworkCore;
using demoWebAPI.Models;
using demoWebAPI.Data.Repositories;

namespace demoWebAPI.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(EcomDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}