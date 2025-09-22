using Microsoft.EntityFrameworkCore;
using ProductCatalogService.Models;


namespace ProductCatalogService.Data
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
    }
}
