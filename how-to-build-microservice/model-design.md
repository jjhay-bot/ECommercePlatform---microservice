# Model Design in a .NET Microservice

Designing your models (entities) is the foundation of a robust microservice. This guide gives you a quick example, best practices, and actionable tips to get started fast.

---

## DTO Usage in API Design

> **Best Practice:**
> For API input and output, use Data Transfer Objects (DTOs) instead of exposing your entity models (e.g., `Product`) directly. DTOs help control which fields are accepted or returned, improve security, and decouple your API contract from your database schema.
>
> - Use `CreateProductDto` for POST (creation)
> - Use `UpdateProductDto` for PUT/PATCH (updates)
> - Use `ProductDto` for GET (responses)
>
> This approach prevents over-posting, hides sensitive/internal fields, and makes future changes easier.

---

## 🚀 Quick Example: Product Model

```csharp
using System.ComponentModel.DataAnnotations;

namespace ProductCatalogService.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(0.01, 10000)]
        public decimal Price { get; set; }
        [Range(0, 1000)]
        public int Stock { get; set; }
        // Example navigation property
        // public int? CategoryId { get; set; }
        // public Category? Category { get; set; }
    }
}
```

---

> **Why start with models?**
> 
> Models define your data structure and drive your database, API, and business logic. Good models = easier code everywhere else!

---

## 1️⃣ When to Create Models
- Before creating your DbContext or controllers
- When you need to define what data your service will manage

## 2️⃣ Where to Place Models
- In a `Models/` folder in your service project
- One `.cs` file per entity (e.g., `Product.cs`, `User.cs`)

## 3️⃣ How to Define a Model
- Use a C# class with properties for each field
- Add data annotations for validation and constraints
- Add navigation properties for relationships (if needed)

---

## 💡 Model Design Best Practices
- Use **meaningful property names**
- Use **appropriate data types**
- Add **validation attributes** for required fields, length, and ranges
- Keep models focused on **data only** (no business logic)
- Add navigation properties for relationships (optional)

---

## ✅ Quick Checklist
- [ ] Model class created in `Models/`
- [ ] Properties defined with correct types
- [ ] Validation attributes added
- [ ] Navigation properties (if needed)
- [ ] Referenced in your DbContext

---

## Using AutoMapper for DTO Mapping

> **Tip:** To avoid repetitive manual mapping between models and DTOs, use [AutoMapper](https://automapper.org/). It automatically maps properties between objects based on conventions and mapping profiles.
>
> **Setup Example:**
> 1. Install the `AutoMapper.Extensions.Microsoft.DependencyInjection` NuGet package.
> 2. Create a mapping profile (e.g., `ProductMappingProfile`):
>
>    ```csharp
>    using AutoMapper;
>    using ProductCatalogService.Models;
>    using ProductCatalogService.DTOs;
>    
>    namespace ProductCatalogService.Mappings
>    {
>        public class ProductMappingProfile : Profile
>        {
>            public ProductMappingProfile()
>            {
>                CreateMap<Product, ProductDto>();
>                CreateMap<CreateProductDto, Product>();
>                CreateMap<UpdateProductDto, Product>();
>            }
>        }
>    }
>    ```
> 3. Register AutoMapper in your service (e.g., in `Program.cs`):
>
>    ```csharp
>    builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
>    ```
> 4. Inject and use `IMapper` in your controllers to map between models and DTOs.

---

## Next Steps
- Reference your models in your DbContext (e.g., `DbSet<Product> Products`)
- Use models in your controllers for CRUD operations

---

[⬅ Back to Step-by-Step Guide](./step-by-step.md)
