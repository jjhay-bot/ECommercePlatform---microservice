# How to Build a Microservice (.NET + MySQL)

This guide summarizes the step-by-step process for building a new microservice in the E-Commerce Platform. It is designed to be generic and reusable for any new microservice (e.g., ProductCatalogService, UserService, OrderService, etc.).

---

## 1. Scaffold the Microservice Project
- Create a new folder for your service (e.g., `UserService/`)
- Scaffold a new .NET Web API project:
  ```bash
  dotnet new webapi -n <ServiceName>
  ```
- Add the project to the solution:
  ```bash
  dotnet sln add <ServiceName>/<ServiceName>.csproj
  ```

## 2. Add Database Support (MySQL + EF Core)
- Add Pomelo.EntityFrameworkCore.MySql and EF Core Design packages:
  ```bash
  dotnet add package Pomelo.EntityFrameworkCore.MySql
  dotnet add package Microsoft.EntityFrameworkCore.Design
  ```
- Create a `Models/` folder and define your entity classes (e.g., `User.cs`)
- Create a `Data/` folder and define your DbContext (e.g., `UserContext : DbContext`)

## 3. Configure Database Connection
- Add a connection string to `appsettings.json` and `appsettings.Development.json`:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=localhost;port=3306;database=<DbName>;user=<User>;password=<Password>;"
    }
  }
  ```
- Register the DbContext in `Program.cs`:
  ```csharp
  builder.Services.AddDbContext<UserContext>(options =>
      options.UseMySql(
          builder.Configuration.GetConnectionString("DefaultConnection"),
          new MySqlServerVersion(new Version(8, 0, 30))
      )
  );
  ```

## 4. Create and Apply Migrations
- In the service folder, run:
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```
- (Optional) Add code to `Program.cs` to auto-apply migrations on startup:
  ```csharp
  using (var scope = app.Services.CreateScope())
  {
      var db = scope.ServiceProvider.GetRequiredService<UserContext>();
      db.Database.Migrate();
  }
  ```

## 5. Implement CRUD Endpoints
- Create a `Controllers/` folder and add a controller (e.g., `UsersController.cs`)
- Implement RESTful endpoints (GET, POST, PUT, DELETE)
- Use dependency injection to access your DbContext

## 6. Test the API
- Create a `.http` file with sample requests for all endpoints
- Use VS Code REST Client, Postman, or curl to test
- Example:
  ```http
  GET http://localhost:5241/api/v1/users
  POST http://localhost:5241/api/v1/users
  ...
  ```

## 7. Document the Service
- Add a `README.md` in the service folder
- Document:
  - Service purpose
  - API endpoints
  - Database schema
  - Setup instructions
  - Known limitations

## 8. Prepare for Scalability
- Design for statelessness (no session state)
- Use async/await for all database operations
- Plan for load balancing and health checks
- Consider adding health check endpoints

## 9. Secure the Service (Future)
- Plan for authentication/authorization (JWT, API Gateway)
- Validate all input data
- Add logging and monitoring

---

**Tip:**
- Use this guide as a checklist for every new microservice you build.
- Adjust folder names, class names, and connection strings as needed for each service.
- Reference the ProductCatalogService for working code examples.

---

_This guide is designed to help you quickly scaffold, implement, and document new microservices in a consistent and scalable way!_
