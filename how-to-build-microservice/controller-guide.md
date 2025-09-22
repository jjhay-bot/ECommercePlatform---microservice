# Controllers in a .NET Microservice

Controllers handle HTTP requests and responses in your microservice. This guide shows you how to create REST API controllers with full CRUD operations and best practices.

---

## DTO Usage in Controllers

> **Note:**
> Always use DTOs (Data Transfer Objects) for request and response bodies in your controllers. This ensures only intended fields are processed and returned, and keeps your API contract stable even if your database models change.
>
> - Accept DTOs for POST/PUT/PATCH endpoints
> - Return DTOs for GET endpoints if you want to control the response shape
>
> Refer to the model design documentation for more details and examples.

---

## 🚀 Quick Example: ProductsController

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogService.Data;
using ProductCatalogService.Models;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductContext _context;

    public ProductsController(ProductContext context)
    {
        _context = context;
    }

    // GET: api/v1/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // GET: api/v1/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    // POST: api/v1/products
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/v1/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/v1/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

---

## 📋 Controller Structure

### Required Attributes
- **`[ApiController]`** - Enables automatic model validation and API features
- **`[Route("api/v1/[controller]")]`** - Sets the base URL pattern

### Constructor Injection
```csharp
private readonly ProductContext _context;

public ProductsController(ProductContext context)
{
    _context = context;
}
```

### Action Method Patterns
- Use `async Task<ActionResult<T>>` for methods returning data
- Use `async Task<IActionResult>` for methods without return data
- Always use `await` with database operations

---

## 🔧 HTTP Method Mapping

| HTTP Method | Route Pattern | Action | Returns |
|-------------|---------------|--------|---------|
| GET | `/api/v1/products` | Get all items | 200 + data |
| GET | `/api/v1/products/{id}` | Get single item | 200 + data or 404 |
| POST | `/api/v1/products` | Create new item | 201 + created item |
| PUT | `/api/v1/products/{id}` | Update existing | 204 or 400/404 |
| DELETE | `/api/v1/products/{id}` | Delete item | 204 or 404 |

---

## 💡 Best Practices

### Status Codes
- **200 OK** - Successful GET requests
- **201 Created** - Successful POST requests
- **204 No Content** - Successful PUT/DELETE requests
- **400 Bad Request** - Invalid input
- **404 Not Found** - Resource doesn't exist

### Error Handling
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Product>> GetProduct(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) 
    {
        return NotFound(); // Returns 404
    }
    return product; // Returns 200
}
```

### Validation
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> PutProduct(int id, Product product)
{
    if (id != product.Id) 
    {
        return BadRequest(); // Security check
    }
    // ... rest of method
}
```

---

## ✅ Quick Checklist

- [ ] Controller class created in `Controllers/` folder
- [ ] `[ApiController]` attribute added
- [ ] Route pattern defined with versioning (`api/v1/`)
- [ ] DbContext injected via constructor
- [ ] All 5 CRUD endpoints implemented (GET, GET by ID, POST, PUT, DELETE)
- [ ] Async/await used for database operations
- [ ] Proper HTTP status codes returned
- [ ] Error handling for not found/bad requests

---

## Next Steps
- Test endpoints using `.http` files or Postman
- Add input validation and error handling
- Consider adding filtering, sorting, and pagination for GET endpoints

---

For more, see the [Data Folder Guide](./data-folder.md) and [Migration Examples](./migration-examples.md).
