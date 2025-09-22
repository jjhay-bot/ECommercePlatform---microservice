namespace ProductCatalogService.DTOs
{
    // DTO for returning product data (does not include Stock)
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = "--";
        public decimal Price { get; set; }
        // Stock is intentionally omitted
    }

    // DTO for creating a product
    public class CreateProductDto
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = "--";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    // DTO for updating a product (PUT)
    public class UpdateProductDto
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = "--";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
