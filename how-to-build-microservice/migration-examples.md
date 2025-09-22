# Migration Examples: Schema and Data Updates

This guide shows real examples of migrations for both schema changes and updating existing data, based on our ProductCatalogService.

---

## 🚀 Example: Updating Default Values

### Scenario
We want to change the default value for the `Description` field from empty string to `"--"` for new products, and update existing empty descriptions.

### Step 1: Update Your Model
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    // Changed from: public string Description { get; set; } = string.Empty;
    public string Description { get; set; } = "--";  // New default
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

### Step 2: Create Schema Migration
```bash
cd ECommercePlatform/ProductCatalogService
dotnet ef migrations add UpdateDescriptionDefault
dotnet ef database update
```

**Result**: New products created through C# code will have `"--"` as default description.

---

## 📝 Step 3: Update Existing Data (Data Migration)

### Create Data Migration
```bash
dotnet ef migrations add UpdateExistingDescriptions
```

### Edit the Migration File
In `Migrations/YYYYMMDD_UpdateExistingDescriptions.cs`:

```csharp
public partial class UpdateExistingDescriptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Update existing empty descriptions to "--"
        migrationBuilder.Sql("UPDATE Products SET Description = '--' WHERE Description = '' OR Description IS NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Optionally revert the change
        migrationBuilder.Sql("UPDATE Products SET Description = '' WHERE Description = '--';");
    }
}
```

### Apply Data Migration
```bash
dotnet ef database update
```

**Result**: All existing products with empty descriptions now have `"--"`.

---

## 🔍 Before and After

### Before Migration
```
+----+---------------------+----------------------+-------+
| Id | Name                | Description          | Stock |
+----+---------------------+----------------------+-------+
|  4 | Sample Product      | Sample description   |   100 |
|  5 | JHAY Sample Product | JHAY sample product. |    10 |
|  6 | Sample Product      |                      |   100 |  <-- Empty
+----+---------------------+----------------------+-------+
```

### After Migration
```
+----+---------------------+----------------------+-------+
| Id | Name                | Description          | Stock |
+----+---------------------+----------------------+-------+
|  4 | Sample Product      | Sample description   |   100 |
|  5 | JHAY Sample Product | JHAY sample product. |    10 |
|  6 | Sample Product      | --                   |   100 |  <-- Updated!
+----+---------------------+----------------------+-------+
```

---

## 💡 Key Points

### Schema vs Data Migrations
- **Schema Migration**: Changes table structure (add columns, change types, etc.)
- **Data Migration**: Updates existing data in tables

### When to Use Data Migrations
- Updating default values for existing records
- Migrating data formats (e.g., splitting full names into first/last)
- Cleaning up invalid data
- Setting initial values for new required fields

### Best Practices
- Always include a `Down` method to reverse changes
- Test migrations on a copy of production data first
- Back up your database before applying migrations
- Use transactions for complex data updates

---

## 🛠️ Common Data Migration Patterns

### Set Default for New Required Field
```csharp
migrationBuilder.Sql("UPDATE Users SET Status = 'Active' WHERE Status IS NULL;");
```

### Update Based on Conditions
```csharp
migrationBuilder.Sql("UPDATE Products SET Category = 'General' WHERE Category = '' AND Price < 10;");
```

### Copy Data Between Columns
```csharp
migrationBuilder.Sql("UPDATE Users SET FullName = CONCAT(FirstName, ' ', LastName) WHERE FullName IS NULL;");
```

---

For more migration commands, see the [MySQL/SQL Cheat Sheet](./mysql-cheatsheet.md).
