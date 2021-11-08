using System.Linq;
using FastFood.Core.ViewModels.Categories;
using FastFood.Core.ViewModels.Employees;
using FastFood.Core.ViewModels.Items;
using FastFood.Core.ViewModels.Orders;

namespace FastFood.Core.MappingConfiguration
{
    using AutoMapper;
    using FastFood.Models;
    using ViewModels.Positions;

    public class FastFoodProfile : Profile
    {
        public FastFoodProfile()
        {
            //Positions
            this.CreateMap<CreatePositionInputModel, Position>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.PositionName));

            this.CreateMap<Position, PositionsAllViewModel>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.Name));
            //

            //Categories
            this.CreateMap<CreateCategoryInputModel, Category>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.CategoryName));

            this.CreateMap<Category, CategoryAllViewModel>();
            //

            //Items
            this.CreateMap<CreateItemInputModel, Item>();

            this.CreateMap<Item, ItemsAllViewModels>()
                .ForMember(x => x.Category, y => y.MapFrom(s => s.Category.Name));

            this.CreateMap<Item, CreateItemViewModel>()
                .ForMember(x => x.CategoryId, y => y.MapFrom(s => s.CategoryId));

            this.CreateMap<Category, CreateItemViewModel>()
                .ForMember(x => x.CategoryId, y => y.MapFrom(s => s.Id));
            //


            //Employees
            this.CreateMap<RegisterEmployeeInputModel, Employee>()
                .ForMember(x => x.PositionId, y => y.MapFrom(s => s.PositionId));

            this.CreateMap<Employee,RegisterEmployeeInputModel>()
                .ForMember(x=>x.PositionId,y=>y.MapFrom(s=>s.PositionId));

            this.CreateMap<Employee, EmployeesAllViewModel>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.Position.Name))
                .ForMember(x => x.Age, y => y.MapFrom(s => s.Age))
                .ForMember(x => x.Address, y => y.MapFrom(s => s.Address))
                .ForMember(x => x.Position, y => y.MapFrom(s => s.Position));

            this.CreateMap<Employee, RegisterEmployeeViewModel>()
                .ForMember(x => x.PositionId, y => y.MapFrom(s => s.PositionId));

            //

            //Orders
            this.CreateMap<CreateOrderInputModel, Order>()
                .ForMember(x => x.Customer, y => y.MapFrom(s => s.Customer))
                .ForMember(x => x.EmployeeId, y => y.MapFrom(s => s.EmployeeId));

            this.CreateMap<Order, OrderAllViewModel>()
                .ForMember(x => x.OrderId, y => y.MapFrom(s => s.OrderItems.Select(x => x.OrderId)))
                .ForMember(x => x.Customer, y => y.MapFrom(s => s.Customer))
                .ForMember(x => x.Employee, y => y.MapFrom(s => s.Employee))
                .ForMember(x => x.DateTime, y => y.MapFrom(s => s.DateTime));

            this.CreateMap<OrderItem, CreateOrderInputModel>()
                .ForMember(x => x.Quantity, y => y.MapFrom(s => s.Quantity))
                .ForMember(x => x.ItemId, y => y.MapFrom(s => s.ItemId));

            //this.CreateMap<Order, CreateOrderViewModel>()
            //    .ForMember(x => x.Items, y => y.MapFrom(s => s.OrderItems))
            //    .ForMember(x => x.Employees.Select(k => k), y => y.MapFrom(s => s.Employee));
        }
    }
}
