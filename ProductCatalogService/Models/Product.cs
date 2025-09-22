namespace ProductCatalogService.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        // public string Description { get; set; } = string.Empty;
        //. test migration for default value = "--"
        public string Description { get; set; } = "--";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
