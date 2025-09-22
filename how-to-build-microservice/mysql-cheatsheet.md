# MySQL/SQL Cheat Sheet for Microservices

Quick reference for common MySQL/SQL commands when working with .NET microservices and Entity Framework Core.

---

## 🚀 Quick Start Commands

### Connect to MySQL
```bash
mysql -u root -p
mysql -u username -p -h hostname
```

### Basic Database Operations
```sql
-- Show all databases
SHOW DATABASES;

-- Create a database
CREATE DATABASE ProductCatalogDb;

-- Use/select a database
USE ProductCatalogDb;

-- Drop a database (careful!)
DROP DATABASE ProductCatalogDb;
```

---

## 📋 Table Operations

### View Tables and Structure
```sql
-- Show all tables in current database
SHOW TABLES;

-- Describe table structure
DESCRIBE Products;
DESC Products;

-- Show detailed table info
SHOW CREATE TABLE Products;
```

### Basic CRUD Operations
```sql
-- SELECT (Read)
SELECT * FROM Products;
SELECT * FROM Products LIMIT 10;
SELECT Name, Price FROM Products WHERE Price > 10;

-- INSERT (Create)
INSERT INTO Products (Name, Description, Price, Stock)
VALUES ('Sample Product', 'Sample description', 9.99, 100);

-- UPDATE (Update)
UPDATE Products SET Price = 15.99 WHERE Id = 1;

-- DELETE (Delete)
DELETE FROM Products WHERE Id = 1;
DELETE FROM Products WHERE Price < 5;
```

---

## 🔍 Useful Queries

### Filtering and Sorting
```sql
-- WHERE conditions
SELECT * FROM Products WHERE Price BETWEEN 10 AND 50;
SELECT * FROM Products WHERE Name LIKE '%Sample%';
SELECT * FROM Products WHERE Stock > 0;

-- ORDER BY
SELECT * FROM Products ORDER BY Price DESC;
SELECT * FROM Products ORDER BY Name ASC, Price DESC;

-- COUNT and aggregates
SELECT COUNT(*) FROM Products;
SELECT AVG(Price), MAX(Price), MIN(Price) FROM Products;
```

### Data Types
```sql
-- Common data types for microservices
INT                 -- Integer IDs
VARCHAR(255)        -- Short strings
TEXT               -- Long text (descriptions)
DECIMAL(10,2)      -- Prices, currency
DATETIME           -- Timestamps
BOOLEAN            -- True/false flags
```

---

## ⚡ Quick Fixes

### Connection Issues
```sql
-- Check current user
SELECT USER();

-- Show connection info
SHOW PROCESSLIST;

-- Reset password (if needed)
ALTER USER 'root'@'localhost' IDENTIFIED BY 'newpassword';
```

### Development Helpers
```sql
-- Clear all data (keep structure)
TRUNCATE TABLE Products;

-- Reset auto-increment
ALTER TABLE Products AUTO_INCREMENT = 1;

-- Backup table structure
CREATE TABLE Products_backup LIKE Products;
```

---

## 🛠️ EF Core Integration

### Migration-Related Checks
```sql
-- Check migration history
SELECT * FROM __EFMigrationsHistory;

-- See what EF Core created
SHOW TABLES LIKE '__EF%';
```

---

## 💡 Best Practices
- Always back up before making changes
- Use transactions for multiple related changes
- Test queries on small datasets first
- Use LIMIT when exploring large tables
- Be careful with DELETE and UPDATE without WHERE clauses

---

For more, see the [Data Folder Guide](./data-folder.md) and [EF Core Quick Reference](./efcore-quickref.md).
