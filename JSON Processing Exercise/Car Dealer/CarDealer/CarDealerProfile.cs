using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.DTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<CarImport, Car>()
                .ForMember(x => x.Make, y => y.MapFrom(s => s.Make))
                .ForMember(x => x.Model, y => y.MapFrom(s => s.Model))
                .ForMember(x => x.TravelledDistance, y => y.MapFrom(s => s.TravelledDistance))
                .ForMember(x => x.PartCars, y => y.MapFrom(s => s.Parts));

            CreateMap<CustomerExport, Customer>();
        }
    }
}
