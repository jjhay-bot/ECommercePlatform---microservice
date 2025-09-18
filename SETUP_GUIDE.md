# E-Commerce Platform Microservices Setup Guide

This document outlines the step-by-step procedure to set up and develop the E-Commerce Platform microservices using .NET Web API and MySQL. It serves as a reference for initial setup, development workflow, and future enhancements.

## Prerequisites

- .NET SDK (version 7.0 or later) installed
- MySQL server installed and running on local machine
- MySQL client tools (e.g., MySQL Workbench) for database management
- IDE (e.g., Visual Studio, VS Code)
- Git for version control

## Folder Structure

```text
ECommercePlatform/
├── ProductCatalogService/
├── OrderService/
├── PaymentService/
├── UserService/
├── InventoryService/
└── SETUP_GUIDE.md  # this document
```

## Step 1: Create Solution and Projects

1. Open a terminal in the `ECommercePlatform` folder.
2. Create a new solution:
   ```bash
   dotnet new sln -n ECommercePlatform
   ```
3. For each microservice, scaffold a .NET Web API project:
   ```bash
   dotnet new webapi -n ProductCatalogService
   dotnet new webapi -n OrderService
   dotnet new webapi -n PaymentService
   dotnet new webapi -n UserService
   dotnet new webapi -n InventoryService
   ```
4. Add each project to the solution:
   ```bash
   dotnet sln add */*.csproj
   ```

## Step 2: Configure Database (MySQL)

### 2.1 Install MySQL EF Core Provider

In each service folder, run:
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### 2.2 Add Connection String

In `appsettings.json` of each service, add (replace credentials as needed):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=ServiceDb;user=root;password=YourPassword;"
  }
}
```

### 2.3 Create DbContext and Models

1. In `Models/`, define your entities (e.g., `Product.cs`).
2. Create a `Data` folder and add `ServiceDbContext.cs`:
   ```csharp
   public class ServiceDbContext : DbContext
   {
       public DbSet<Product> Products { get; set; }
       public ServiceDbContext(DbContextOptions<ServiceDbContext> options)
           : base(options) {}
   }
   ```
3. In `Program.cs`, register EF Core context:
   ```csharp
   builder.Services.AddDbContext<ServiceDbContext>(options =>
       options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
           new MySqlServerVersion(new Version(8, 0, 30))));
   ```

## Step 3: Scaffold Controllers and CRUD Endpoints

1. Add controllers in `Controllers/` folder.
2. Example for Products:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class ProductsController : ControllerBase
   {
       private readonly ServiceDbContext _context;
       public ProductsController(ServiceDbContext context) { _context = context; }
       // GET, POST, PUT, DELETE actions
   }
   ```
3. Test endpoints using Postman or HTTP client.

## Step 4: Run and Test Service Locally

1. In each service folder, run:
   ```bash
   dotnet run
   ```
2. Ensure database tables are created and endpoints respond.

## Step 5: Future Services and Inter-Service Communication

- Order Service should call Inventory Service to reserve stock and Payment Service to process payments.
- Use `HttpClient` or a messaging bus (e.g., RabbitMQ) for decoupled communication.
- Secure inter-service calls with JWT tokens and an API gateway if needed.

## Notes & Best Practices

- **Database Per Service:** Keep a separate database for each microservice to enforce loose coupling.
- **Migrations:** Use `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to manage schema.
- **Logging:** Use built-in `ILogger<T>` in each service for consistent logging.
- **Configuration:** Store secrets securely (e.g., environment variables, Azure Key Vault).
- **Docker:** Containerize services for consistent deployment and testing.

---

_End of Setup Guide._
