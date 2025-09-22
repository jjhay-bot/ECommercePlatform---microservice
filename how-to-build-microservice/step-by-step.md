# Step-by-Step Guide: Building a Microservice

This guide summarizes the process for building a new .NET microservice. Use it as a checklist for every new service (User, Order, Payment, etc.).

---

1. **Scaffold the Project**
   - `dotnet new webapi -n <ServiceName>`
   - `dotnet sln add <ServiceName>/<ServiceName>.csproj`

2. **Add Database Support**
   - `dotnet add package Pomelo.EntityFrameworkCore.MySql`
   - `dotnet add package Microsoft.EntityFrameworkCore.Design`
   - Create `Models/` and `Data/` folders
   - Define your entity and DbContext
   - > **See:** [Model Design Guide](./model-design.md), [Data Folder Guide](./data-folder.md)

3. **Configure Database Connection**
   - Add connection string to `appsettings.json`
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ProductCatalogDb;User=root;Password=yourpassword;"
     }
     ```
   - Register DbContext in `Program.cs`
     ```csharp
     builder.Services.AddDbContext<ProductContext>(options =>
         options.UseMySql(
             builder.Configuration.GetConnectionString("DefaultConnection"),
             ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
         )
     );
     ```

4. **Create and Apply Migrations**
   - `dotnet ef migrations add InitialCreate`
   - `dotnet ef database update`
   - (Optional) Auto-apply migrations in `Program.cs`
   > **Note:** Every time you change your model classes (add, remove, or update properties), you must create a new migration and update the database. If you forget, your code and database can get out of sync, causing errors.
   > 
   > **Advanced:** You can also use migrations to update existing data, not just the schema. For example, you can write SQL in a migration to set a default value for all existing rows. This is called a data migration.
   > 
   > **See:** [Migration Examples](./migration-examples.md) for real examples of schema and data migrations.

5. **Implement CRUD Endpoints**
   - Add controller(s) in `Controllers/`
   - Use `[ApiController]` and `[Route("api/v1/[controller]")]` attributes
   - Inject DbContext via constructor
   - Implement standard REST endpoints:
     - `GET /api/v1/products` - Get all items
     - `GET /api/v1/products/{id}` - Get single item
     - `POST /api/v1/products` - Create new item
     - `PUT /api/v1/products/{id}` - Update existing item
     - `DELETE /api/v1/products/{id}` - Delete item
   - Use async/await for database operations
   - Return appropriate HTTP status codes (200, 201, 404, etc.)
   - > **See:** [Controller Guide](./controller-guide.md) for complete examples and best practices

6. **Test the API**
   - Start the service: `dotnet run --project <ServiceName>/<ServiceName>.csproj` (from solution root)
   - Or navigate to service folder and run: `dotnet run`
   - Create a `.http` file with sample requests
   - Use REST Client, Postman, or curl

7. **Document the Service**
   - Add a `README.md` in the service folder
   - Document endpoints, setup, and known issues

8. **Prepare for Scalability**
   - Design for statelessness
   - Use async/await for DB operations
   - Plan for health checks and load balancing

9. **Secure the Service (Future)**
   - Plan for authentication/authorization
   - Validate all input
   - Add logging and monitoring

---

[⬅ Back to Documentation Hub](./README.md)
