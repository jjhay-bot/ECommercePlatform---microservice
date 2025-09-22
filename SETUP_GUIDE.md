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

## Understanding Pomelo.EntityFrameworkCore.MySql

### What is Pomelo?

**Pomelo.EntityFrameworkCore.MySql** is a database provider that enables Entity Framework Core (EF Core) to work with MySQL databases. Think of it as a "translator" between your .NET application and MySQL.

### Why Do We Need Pomelo?

#### 1. **Entity Framework Core Doesn't Support MySQL Natively**
- EF Core by default only supports SQL Server, SQLite, and InMemory databases
- To connect to MySQL, you need a specialized provider
- Pomelo is the most popular and well-maintained MySQL provider for EF Core

#### 2. **The Complete Picture: How Everything Connects**

```text
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Your C# App   │ -> │ Pomelo Provider │ -> │ MySQL Protocol  │ -> │ MySQL Server    │
│                 │    │ (Translates)    │    │ (Network/Socket)│    │ (Running on     │
│ LINQ Queries    │    │ C# -> SQL       │    │                 │    │  port 3306)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
```

**What this means:**

- **Your C# App**: Writes LINQ queries and entity operations
- **Pomelo Provider**: Translates C# code into MySQL-compatible SQL
- **MySQL Protocol**: Network communication layer between app and database
- **MySQL Server**: The actual database engine running on your machine

Without Pomelo, this chain is broken - EF Core doesn't know how to "speak" to MySQL.

#### 3. **What Pomelo Does Behind the Scenes**

**SQL Translation:**
```csharp
// Your C# LINQ query
var products = await _context.Products
    .Where(p => p.Price > 10)
    .ToListAsync();
```

**Pomelo translates this to MySQL-specific SQL:**
```sql
SELECT `p`.`Id`, `p`.`Name`, `p`.`Description`, `p`.`Price`, `p`.`Stock`
FROM `Products` AS `p`
WHERE `p`.`Price` > 10.0
```

**Data Type Mapping:**
- C# `string` → MySQL `VARCHAR` or `TEXT`
- C# `decimal` → MySQL `DECIMAL`
- C# `DateTime` → MySQL `DATETIME`
- C# `bool` → MySQL `TINYINT(1)`

#### 4. **Alternatives to Pomelo**

| Provider | Pros | Cons |
|----------|------|------|
| **Pomelo** | ✅ Free, Open Source ✅ Actively maintained ✅ High performance ✅ Full EF Core features | ❌ Community-driven (not Microsoft) |
| **Oracle's MySQL.EntityFrameworkCore** | ✅ Official Oracle provider | ❌ Limited EF Core features ❌ Less performant |
| **Raw ADO.NET** | ✅ Maximum control ✅ High performance | ❌ No ORM benefits ❌ More code to write |
| **Dapper** | ✅ Lightweight ✅ Fast | ❌ No change tracking ❌ Manual SQL writing |

### How Pomelo Works in Your Project

#### 1. **Package Installation**
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

#### 2. **Configuration in Program.cs**
```csharp
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 30)) // MySQL version
    )
);
```

#### 3. **What Happens When You Run Migrations**
```bash
dotnet ef migrations add InitialCreate
```

Pomelo generates MySQL-specific migration code:
```sql
CREATE TABLE `Products` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Price` decimal(65,30) NOT NULL,
    `Stock` int NOT NULL,
    CONSTRAINT `PK_Products` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;
```

### Key Benefits of Using Pomelo

1. **Object-Relational Mapping (ORM):**
   - Write C# code instead of SQL
   - Automatic type conversion
   - IntelliSense support

2. **Change Tracking:**
   - EF Core automatically detects entity changes
   - Generates optimal UPDATE statements

3. **Migrations:**
   - Version control for your database schema
   - Automatic database updates

4. **LINQ Support:**
   - Write database queries using familiar C# syntax
   - Compile-time query validation

5. **Performance Optimizations:**
   - Connection pooling
   - Query optimization
   - Lazy loading support

### Real Example: Before and After Pomelo

**Without Pomelo (Raw ADO.NET):**
```csharp
using var connection = new MySqlConnection(connectionString);
var command = new MySqlCommand(
    "SELECT Id, Name, Price FROM Products WHERE Price > @price", 
    connection);
command.Parameters.AddWithValue("@price", 10);
connection.Open();
var reader = command.ExecuteReader();
var products = new List<Product>();
while (reader.Read())
{
    products.Add(new Product
    {
        Id = reader.GetInt32("Id"),
        Name = reader.GetString("Name"),
        Price = reader.GetDecimal("Price")
    });
}
```

**With Pomelo + EF Core:**
```csharp
var products = await _context.Products
    .Where(p => p.Price > 10)
    .ToListAsync();
```

Much cleaner and more maintainable! 🎉

## Step 2: Configure Database (MySQL)

### 2.0: Prepare MySQL Database and User

Before updating the connection string, you must create the database and a dedicated MySQL user with the correct permissions.

1.  **Log in to MySQL as root:** Open your terminal and run the following command. You will be prompted for your MySQL root password.

    ```bash
    mysql -u root -p
    ```

2.  **Execute SQL commands:** Once you are at the `mysql>` prompt, run the following SQL statements. **Remember to replace `'YourPassword'` with a real, secure password.**

    ```sql
    CREATE DATABASE ProductCatalogDb;
    CREATE USER 'catalog_user'@'localhost' IDENTIFIED BY 'YourPassword';
    GRANT ALL PRIVILEGES ON ProductCatalogDb.* TO 'catalog_user'@'localhost';
    FLUSH PRIVILEGES;
    EXIT;
    ```

This process creates the `ProductCatalogDb` database and a `catalog_user` that your application will use to connect to it.

### 2.1 Install MySQL EF Core Provider

In each service folder, run:
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### 2.2 Add Connection String

In `appsettings.Development.json`, add the connection string, making sure the `password` matches the one you set in the SQL command above.

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

### 2.4 Generate and Apply Migrations

In the `ProductCatalogService` folder, ensure `dotnet-ef` is on your PATH and run:

```bash
export PATH="$PATH:/Users/<your-user>/.dotnet/tools"
cd ProductCatalogService
dotnet ef migrations add InitialCreate --context ProductContext
dotnet ef database update --context ProductContext
```

### 2.5 Run the Service

From the solution root folder, start the Web API with:
```bash
dotnet run --project ProductCatalogService/ProductCatalogService.csproj
```

Or navigate to the service folder first:
```bash
cd ProductCatalogService
dotnet run
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

## REST API Standards and Best Practices

This platform follows industry-standard REST conventions to ensure consistency, maintainability, and ease of integration.

### API Endpoint Structure

```text
/api/v1/{resource}
```

**Example:**

- ProductCatalogService: `/api/v1/products`
- OrderService: `/api/v1/orders`
- UserService: `/api/v1/users`

### HTTP Methods and Operations

| HTTP Method | Endpoint Pattern | Purpose | Example |
|-------------|------------------|---------|---------|
| `GET` | `/api/v1/products` | Get all products | Retrieve product list |
| `GET` | `/api/v1/products/{id}` | Get specific product | Retrieve product by ID |
| `POST` | `/api/v1/products` | Create new product | Add new product |
| `PUT` | `/api/v1/products/{id}` | Update existing product | Modify product data |
| `DELETE` | `/api/v1/products/{id}` | Delete product | Remove product |

### REST Naming Conventions

1. **Resource Names:**
   - Use **plural nouns** (e.g., `products`, `orders`, `users`)
   - Use **lowercase** (e.g., `products` not `Products`)
   - Use **kebab-case** for multi-word resources (e.g., `order-items`)

2. **API Versioning:**
   - Include version in URL: `/api/v1/products`
   - Allows backward compatibility when making breaking changes

3. **HTTP Status Codes:**
   - `200 OK` - Successful GET, PUT
   - `201 Created` - Successful POST
   - `204 No Content` - Successful DELETE
   - `400 Bad Request` - Invalid request data
   - `404 Not Found` - Resource not found
   - `500 Internal Server Error` - Server error

### Controller Implementation Example

```csharp
[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    // GET /api/v1/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    
    // GET /api/v1/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    
    // POST /api/v1/products
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    
    // PUT /api/v1/products/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, Product product)
    
    // DELETE /api/v1/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
}
```

### Request/Response Format

**Request Headers:**

```text
Content-Type: application/json
Accept: application/json
```

**POST/PUT Request Body Example:**

```json
{
  "name": "Sample Product",
  "description": "Product description",
  "price": 29.99,
  "stock": 100
}
```

**GET Response Example:**

```json
{
  "id": 1,
  "name": "Sample Product",
  "description": "Product description",
  "price": 29.99,
  "stock": 100
}
```

### API Testing with .http Files

Each service includes a `.http` file for easy API testing:

```http
@ServiceAddress = http://localhost:5241

# Get all products
GET {{ServiceAddress}}/api/v1/products
Accept: application/json

###
# Create a product
POST {{ServiceAddress}}/api/v1/products
Content-Type: application/json

{
  "name": "Test Product",
  "description": "Test description",
  "price": 19.99,
  "stock": 50
}
```

### Consistency Across Services

All microservices should follow the same patterns:

- Same URL structure (`/api/v1/{resource}`)
- Same HTTP methods for CRUD operations
- Same status code conventions
- Same request/response formatting
- Same error handling patterns

This ensures a predictable and developer-friendly API across the entire platform.

## Notes & Best Practices

- **Database Per Service:** Keep a separate database for each microservice to enforce loose coupling.
- **Migrations:** Use `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to manage schema.
- **Logging:** Use built-in `ILogger<T>` in each service for consistent logging.
- **Configuration:** Store secrets securely (e.g., environment variables, Azure Key Vault).
- **Docker:** Containerize services for consistent deployment and testing.

---

_End of Setup Guide._

## Understanding the ProductsController - Complete Code Walkthrough

This section breaks down every part of the ProductsController to help you understand what each piece does and why it's important.

### File Structure and Imports

```csharp
// IMPORTANT: These 'using' statements import the functionality we need
using Microsoft.AspNetCore.Mvc;        // For [ApiController], [HttpGet], ActionResult, etc.
using Microsoft.EntityFrameworkCore;   // For .ToListAsync(), .FindAsync(), EntityState, etc.
using ProductCatalogService.Data;      // For ProductContext (our database context)
using ProductCatalogService.Models;    // For Product model
```

**What each import does:**
- `Microsoft.AspNetCore.Mvc` - Provides web API functionality (controllers, HTTP methods, status codes)
- `Microsoft.EntityFrameworkCore` - Provides database operations (async methods, change tracking)
- `ProductCatalogService.Data` - Your custom database context
- `ProductCatalogService.Models` - Your custom Product model

### Controller Class Declaration

```csharp
namespace ProductCatalogService.Controllers
{
    [ApiController]                     // Tells .NET this is an API controller
    [Route("api/v1/products")]         // Base URL for all methods: /api/v1/products
    public class ProductsController : ControllerBase  // Inherits API functionality
```

**Key Points:**
- `[ApiController]` - Enables automatic model validation, problem details for errors, and other API features
- `[Route("api/v1/products")]` - Sets the base URL pattern for all endpoints in this controller
- `: ControllerBase` - Inherits all the HTTP functionality (status codes, responses, etc.)

### Dependency Injection Setup

```csharp
    // IMPORTANT: This field stores our database connection for ALL methods
    private readonly ProductContext _context;

    // IMPORTANT: Constructor runs ONCE when .NET creates this controller
    public ProductsController(ProductContext context)
    {
        _context = context; // Store the database context for use in all methods
    }
```

**What happens here:**
1. `private readonly` - Creates a field that can only be set once (in constructor)
2. Constructor parameter - .NET automatically provides ProductContext (configured in Program.cs)
3. Assignment - Stores the context so all methods can use it

### CRUD Operations Breakdown

#### GET All Products
```csharp
    [HttpGet]  // Responds to: GET /api/v1/products
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // Pomelo translates this to: SELECT * FROM Products
        return await _context.Products.ToListAsync();
    }
```

**Important notes:**
- `[HttpGet]` - Maps to HTTP GET method
- `async Task<ActionResult<IEnumerable<Product>>>` - Returns a list of products asynchronously
- `ToListAsync()` - Gets all products from database without blocking the thread

#### GET Single Product
```csharp
    [HttpGet("{id}")]  // Responds to: GET /api/v1/products/5
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        // Pomelo translates to: SELECT * FROM Products WHERE Id = @id
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound();  // Returns HTTP 404
        return product;         // Returns HTTP 200 with product data
    }
```

**Important notes:**
- `{id}` - Captures the ID from the URL
- `FindAsync(id)` - Efficient way to find by primary key
- `NotFound()` - Returns proper HTTP 404 status code
- Always check for null before returning data

#### POST Create Product
```csharp
    [HttpPost]  // Responds to: POST /api/v1/products
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        // Step 1: Add to context (in memory only)
        _context.Products.Add(product);
        
        // Step 2: Save to database
        // Pomelo translates to: INSERT INTO Products (...) VALUES (...)
        await _context.SaveChangesAsync();
        
        // Return HTTP 201 Created with location header
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
```

**Important notes:**
- Two-step process: Add to context, then save changes
- `SaveChangesAsync()` - Actually executes the database operation
- `CreatedAtAction()` - Returns HTTP 201 with location header pointing to new resource

#### PUT Update Product
```csharp
    [HttpPut("{id}")]  // Responds to: PUT /api/v1/products/5
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        // Security check - URL ID must match product ID
        if (id != product.Id)
            return BadRequest();

        // Tell EF Core this product has been modified
        _context.Entry(product).State = EntityState.Modified;
        
        try
        {
            // Pomelo translates to: UPDATE Products SET ... WHERE Id = @id
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle case where product was deleted by another user
            if (!ProductExists(id))
                return NotFound();
            throw;  // Re-throw other errors
        }

        return NoContent();  // Returns HTTP 204
    }
```

**Important notes:**
- Security validation ensures URL ID matches body ID
- `EntityState.Modified` - Tells EF Core to update this entity
- Concurrency handling - Deals with race conditions
- `NoContent()` - Returns HTTP 204 (success, no response body)

#### DELETE Product
```csharp
    [HttpDelete("{id}")]  // Responds to: DELETE /api/v1/products/5
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // First, find the product
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        // Mark for deletion
        _context.Products.Remove(product);
        
        // Execute deletion: DELETE FROM Products WHERE Id = @id
        await _context.SaveChangesAsync();
        
        return NoContent();  // Returns HTTP 204
    }
```

**Important notes:**
- Must find the entity first before deleting
- `Remove()` - Marks entity for deletion
- `SaveChangesAsync()` - Actually executes the DELETE

#### Helper Method
```csharp
    // Helper method to check if product exists
    private bool ProductExists(int id) => _context.Products.Any(e => e.Id == id);
    // Translates to: SELECT COUNT(*) FROM Products WHERE Id = @id > 0
```

### Key Patterns and Best Practices

#### 1. **HTTP Status Code Usage**
- `200 OK` - Successful GET operations
- `201 Created` - Successful POST operations  
- `204 No Content` - Successful PUT/DELETE operations
- `400 Bad Request` - Invalid input data
- `404 Not Found` - Resource doesn't exist

#### 2. **Async/Await Pattern**
```csharp
// All database operations are async to prevent blocking
await _context.Products.ToListAsync();     // Non-blocking
await _context.SaveChangesAsync();         // Non-blocking
```

#### 3. **Two-Step Database Operations**
```csharp
_context.Products.Add(product);     // Step 1: Stage the change (in memory)
await _context.SaveChangesAsync();  // Step 2: Execute the change (to database)
```

#### 4. **Error Handling**
- Always check for null entities
- Handle concurrency exceptions
- Return appropriate HTTP status codes
- Validate input parameters

#### 5. **REST Conventions**
- Use appropriate HTTP methods (GET, POST, PUT, DELETE)
- Follow URL patterns (`/api/v1/products`, `/api/v1/products/{id}`)
- Return proper status codes and response formats

This controller follows all .NET Core best practices and provides a solid foundation for building REST APIs! 🚀

## Understanding C# Keywords vs JavaScript
