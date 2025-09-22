using AutoMapper;
using ProductCatalogService.Models;
using ProductCatalogService.DTOs;

namespace ProductCatalogService.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Product -> ProductDto (Stock is ignored)
            CreateMap<Product, ProductDto>();

            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>();

            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>();
        }
    }
}

// In Program.cs or Startup.cs, register AutoMapper with the ProductMappingProfile:
// builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
