using AutoMapper;
using ProductShop.DTO;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            CreateMap<Product, ProductsInRange>()
                .ForMember(x => x.Seller, y => y.MapFrom(p => $"{p.Seller.FirstName} {p.Seller.LastName}"));
            CreateMap<User, UserSoldProducts>()
                .ForMember(x => x.SoldProducts, y => y.MapFrom(u => u.ProductsSold));

            CreateMap<Product, SoldProducts>()
                .ForMember(x => x.BuyerFirstName, y => y.MapFrom(p => p.Buyer.FirstName))
                .ForMember(x => x.BuyerLastName, y => y.MapFrom(p => p.Buyer.LastName));
        }
    }
}
