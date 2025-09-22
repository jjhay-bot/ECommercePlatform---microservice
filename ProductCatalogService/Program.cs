using Microsoft.EntityFrameworkCore;
using ProductCatalogService.Data;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalogService.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

// Configure EF Core with MySQL
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30))
    )
);

// Register AutoMapper with the ProductMappingProfile
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

var app = builder.Build();

// Apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controller routes
app.MapControllers();

app.Run();
