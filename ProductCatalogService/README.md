# ProductCatalogService Documentation

## Overview

The ProductCatalogService is a .NET Web API microservice responsible for managing product information in the e-commerce platform. It provides complete CRUD operations for products and serves as the central catalog for all product-related data.

## Architecture & Design

### Service Boundaries
- **Primary Responsibility**: Product catalog management
- **Data Ownership**: Product information (name, description, price, stock)
- **API Endpoints**: `/api/v1/products`
- **Database**: `ProductCatalogDb` (MySQL)

### Technology Stack
- **.NET 9.0** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **Pomelo.EntityFrameworkCore.MySql** - MySQL provider
- **MySQL 8.0** - Database engine

## Microservice Strengths

### 1. **Single Responsibility Principle**
```
✅ Focused solely on product catalog management
✅ Clear business boundaries
✅ Independent deployment and scaling
✅ Dedicated database per service pattern
```

### 2. **Technology Independence**
```
✅ Can be developed by separate teams
✅ Technology stack can evolve independently
✅ Database schema changes don't affect other services
✅ Can switch databases without impacting other services
```

### 3. **High Cohesion, Low Coupling**
```
✅ All product-related operations in one place
✅ Minimal dependencies on other services
✅ RESTful API provides loose coupling
✅ Stateless design for better scalability
```

### 4. **Performance Optimization**
```
✅ Dedicated database connection pooling
✅ Async/await for non-blocking operations
✅ Efficient EF Core query optimization
✅ Can implement caching strategies independently
```

## Scalability Considerations

### 1. **Horizontal Scaling**
```csharp
// Current: Single instance
ProductCatalogService:5241

// Future: Multiple instances with load balancer
Load Balancer → ProductCatalogService:5241
              → ProductCatalogService:5242
              → ProductCatalogService:5243
```

**Scaling Strategies:**
- **Stateless Design**: No session state, can run multiple instances
- **Database Connection Pooling**: Efficient connection management
- **Read Replicas**: Separate read/write operations for better performance
- **Caching Layer**: Redis/In-Memory caching for frequently accessed products

### 2. **Vertical Scaling**
- **Database Optimization**: Indexing on frequently queried fields
- **Query Optimization**: Use projection for large datasets
- **Memory Management**: EF Core change tracking optimization
- **CPU Optimization**: Async operations prevent thread blocking

### 3. **Data Partitioning**
```sql
-- Future: Partition by category or price range
CREATE TABLE Products_Electronics (...);
CREATE TABLE Products_Clothing (...);
CREATE TABLE Products_Books (...);
```

### 4. **Performance Metrics to Monitor**
- **Response Time**: API endpoint latency
- **Throughput**: Requests per second
- **Database Connections**: Pool utilization
- **Memory Usage**: EF Core context lifecycle
- **CPU Usage**: Query processing load

## Current Limitations

### 1. **Data Consistency**
```
❌ No distributed transaction support
❌ Eventual consistency challenges with other services
❌ No event sourcing for audit trails
❌ Limited conflict resolution for concurrent updates
```

### 2. **Search and Filtering**
```
❌ Basic LINQ queries only
❌ No full-text search capabilities
❌ No advanced filtering (price ranges, categories)
❌ No search relevance ranking
```

### 3. **Security**
```
❌ No authentication/authorization implemented
❌ No API rate limiting
❌ No input validation beyond model binding
❌ No audit logging for sensitive operations
```

### 4. **Resilience**
```
❌ No circuit breaker pattern
❌ No retry policies for database failures
❌ No health checks implemented
❌ No graceful degradation strategies
```

### 5. **Observability**
```
❌ Limited logging implementation
❌ No distributed tracing
❌ No custom metrics collection
❌ No centralized error handling
```

## Inter-Service Communication Patterns

### 1. **Current State: Isolated Service**
```
ProductCatalogService (Port: 5241)
├── GET /api/v1/products
├── POST /api/v1/products
├── PUT /api/v1/products/{id}
└── DELETE /api/v1/products/{id}
```

### 2. **Future Communication Patterns**

#### **Synchronous Communication (HTTP)**
```csharp
// OrderService calling ProductCatalogService
public class OrderService
{
    private readonly HttpClient _httpClient;
    
    public async Task<Product> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"http://productcatalog:5241/api/v1/products/{productId}");
        return await response.Content.ReadFromJsonAsync<Product>();
    }
}
```

#### **Asynchronous Communication (Events)**
```csharp
// ProductCatalogService publishes events
public class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Publish event to message bus
        await _eventBus.PublishAsync(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price
        });
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

#### **Service Discovery Pattern**
```csharp
// Future: Service registry integration
public class ProductCatalogClient
{
    private readonly IServiceDiscovery _serviceDiscovery;
    
    public async Task<Product> GetProductAsync(int id)
    {
        var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync("ProductCatalogService");
        // Make HTTP call to discovered service
    }
}
```

## Authentication & Authorization Integration

### 1. **JWT Token Validation**
```csharp
// Future Program.cs configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://auth-service",
            ValidateAudience = true,
            ValidAudience = "productcatalog-api",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
```

### 2. **Role-Based Access Control**
```csharp
// Future controller with authorization
[ApiController]
[Route("api/v1/products")]
[Authorize] // Require authentication for all endpoints
public class ProductsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous] // Public endpoint for browsing
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    
    [HttpPost]
    [Authorize(Roles = "Admin,ProductManager")] // Admin-only operations
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ProductManager")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Admin-only deletion
    public async Task<IActionResult> DeleteProduct(int id)
}
```

### 3. **API Gateway Integration**
```
Client → API Gateway → Auth Service (JWT validation)
                  ↓
              ProductCatalogService (with validated claims)
```

### 4. **User Context Injection**
```csharp
// Future: User context from JWT claims
public class ProductsController : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        var userId = User.FindFirst("sub")?.Value;
        var userRole = User.FindFirst("role")?.Value;
        
        // Audit logging with user context
        _logger.LogInformation("User {UserId} with role {Role} created product {ProductName}", 
            userId, userRole, product.Name);
        
        // Business logic
    }
}
```

## Future Enhancements

### 1. **Advanced Search Capabilities**
- Elasticsearch integration for full-text search
- Category-based filtering
- Price range queries
- Search autocomplete
- Faceted search results

### 2. **Caching Strategy**
- Redis distributed cache
- In-memory caching for frequently accessed products
- Cache invalidation on product updates
- Cache warming strategies

### 3. **Event-Driven Architecture**
- Product lifecycle events (created, updated, deleted)
- Integration with inventory service
- Price change notifications
- Stock level alerts

### 4. **Advanced Security**
- API rate limiting
- Input validation and sanitization
- SQL injection protection
- Cross-origin resource sharing (CORS)
- API versioning strategy

### 5. **Monitoring & Observability**
- Application Insights integration
- Custom performance counters
- Distributed tracing with OpenTelemetry
- Health check endpoints
- Structured logging with Serilog

## Service Dependencies

### Current Dependencies
```
ProductCatalogService
├── MySQL Database (ProductCatalogDb)
├── Pomelo.EntityFrameworkCore.MySql
└── Microsoft.EntityFrameworkCore
```

### Future Dependencies
```
ProductCatalogService
├── MySQL Database (ProductCatalogDb)
├── Redis Cache
├── Message Bus (RabbitMQ/Azure Service Bus)
├── Auth Service (JWT validation)
├── Service Discovery (Consul/Eureka)
└── API Gateway
```

## Development Workflow

### 1. **Local Development**
```bash
# Start MySQL
mysql.server start

# Run migrations
dotnet ef database update

# Start service
dotnet run
# Service available at: http://localhost:5241
```

### 2. **Testing Strategy**
- **Unit Tests**: Controller logic, business rules
- **Integration Tests**: Database operations, API endpoints
- **Contract Tests**: API contract validation
- **Performance Tests**: Load testing for scalability

### 3. **Deployment Pipeline**
```
Code → Build → Unit Tests → Integration Tests → Package → Deploy
```

This documentation serves as a comprehensive guide for understanding the ProductCatalogService's role in the microservices architecture and its evolution path.
