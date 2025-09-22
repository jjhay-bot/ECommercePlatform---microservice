using ProductCatalogService.Models;
using ProductCatalogService.DTOs;

namespace ProductCatalogService.Mappings
{
    public static class ProductMappers
    {
        public static ProductDto ToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
        }

        public static Product ToProduct(CreateProductDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock
            };
        }

        public static void UpdateProductFromDto(Product product, UpdateProductDto dto)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
        }
    }
}
