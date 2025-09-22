# Data Folder in a .NET Microservice

The `Data/` folder contains classes that manage database access and persistence logic. This is where you define your `DbContext` and, optionally, repository classes.

---

## 🚀 Quick Example: ProductContext

```csharp
using Microsoft.EntityFrameworkCore;
using ProductCatalogService.Models;

namespace ProductCatalogService.Data
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();
    }
}
```

---

## What Goes in the Data Folder?
- **DbContext**: The main class for database access (e.g., `ProductContext`)
- **Repositories** (optional): Classes that encapsulate data access logic for specific entities
- **Migrations**: Auto-generated files for database schema changes

---

## Best Practices
- Keep data access logic here, separate from business logic and controllers
- Use dependency injection to provide `DbContext` to your controllers/services
- Name your context after the service (e.g., `ProductContext` for ProductCatalogService)
- Use one `DbContext` per microservice

---

## Why Organize This Way?
> Keeping all data access code in one place makes your service easier to maintain, test, and scale.

---

For more, see the [EF Core Quick Reference](./efcore-quickref.md).
