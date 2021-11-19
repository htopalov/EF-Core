using AutoMapper;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            CreateMap<UserImport, User>();
            CreateMap<ProductImport, Product>();
            CreateMap<Product, ProductInRangeExport>()
                .ForMember(x => x.Buyer, y => y.MapFrom(x => x.Buyer.FirstName + " " + x.Buyer.LastName));
            CreateMap<Product, UserSoldProductsExport>();
        }
    }
}
