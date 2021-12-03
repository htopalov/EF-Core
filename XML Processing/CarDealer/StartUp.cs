using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CarDealer.Data;
using CarDealer.Dto.Export;
using CarDealer.Dto.Import;
using CarDealer.Models;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var dbContext = new CarDealerContext();

            //var input = File.ReadAllText("../../../Datasets/suppliers.xml");
            //Console.WriteLine(ImportSuppliers(dbContext,input));

            //var input = File.ReadAllText("../../../Datasets/parts.xml");
            //Console.WriteLine(ImportParts(dbContext, input));

            //var input = File.ReadAllText("../../../Datasets/cars.xml");
            //Console.WriteLine(ImportCars(dbContext, input));

            //var input = File.ReadAllText("../../../Datasets/customers.xml");
            //Console.WriteLine(ImportCustomers(dbContext, input));

            //var input = File.ReadAllText("../../../Datasets/sales.xml");
            //Console.WriteLine(ImportSales(dbContext, input));

            //File.WriteAllText("../../../Results/cars.xml", GetCarsWithDistance(dbContext));
            //File.WriteAllText("../../../Results/bmw-cars.xml", GetCarsFromMakeBmw(dbContext));
            //File.WriteAllText("../../../Results/local-suppliers.xml", GetLocalSuppliers(dbContext));
            //File.WriteAllText("../../../Results/cars-and-parts.xml", GetCarsWithTheirListOfParts(dbContext));
            //File.WriteAllText("../../../Results/customers-total-sales.xml", GetTotalSalesByCustomer(dbContext));
            File.WriteAllText("../../../Results/sales-discounts.xml", GetSalesWithAppliedDiscount(dbContext));
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Suppliers");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportSupplierDto>), root);
            var importedSuppliers = new List<ImportSupplierDto>();
            using (var reader = new StringReader(inputXml))
            {
                importedSuppliers = (List<ImportSupplierDto>) serializer.Deserialize(reader);
            }

            var validSuppliers = new List<Supplier>();
            foreach (var supplierDto in importedSuppliers)
            {
                var parseBoolean = Boolean.TryParse(supplierDto.isImporter, out bool result);
                if (!parseBoolean)
                {
                    continue;
                }
                var supplier = new Supplier()
                {
                    Name = supplierDto.Name,
                    IsImporter = result
                };
                validSuppliers.Add(supplier);
            }

            context.Suppliers.AddRange(validSuppliers);
            context.SaveChanges();

            return $"Successfully imported {validSuppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Parts");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportPartDto>), root);
            var importedParts = new List<ImportPartDto>();
            using (var reader = new StringReader(inputXml))
            {
                importedParts = (List<ImportPartDto>)serializer.Deserialize(reader);
            }

            var validParts = new List<Part>();

            foreach (var partDto in importedParts)
            {
                var supplier = context.Suppliers
                    .FirstOrDefault(s => s.Id == partDto.SupplierId);
                if (supplier == null)
                {
                    continue;
                }

                var part = new Part()
                {
                    Name = partDto.Name,
                    Price = partDto.Price,
                    Quantity = partDto.Quantity,
                    Supplier = supplier
                };
                validParts.Add(part);
            }

            context.Parts.AddRange(validParts);
            context.SaveChanges();

            return $"Successfully imported {validParts.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Cars");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportCarDto>), root);

            var importedCars =  new List<ImportCarDto>();

            using (var reader = new StringReader(inputXml))
            {
                importedCars = (List<ImportCarDto>)serializer.Deserialize(reader);
            }

            var validCars = new List<Car>();
            var validCarParts = new List<PartCar>();

            foreach (var carDto in importedCars)
            {
                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance
                };

                var parts = carDto
                    .Parts
                    .Select(p => p.Id)
                    .Where(p => context.Parts.Any(part => part.Id == p))
                    .Distinct();

                foreach (var partId in parts)
                {
                    var carPart = new PartCar()
                    {
                        PartId = partId,
                        Car = car
                    };

                    validCarParts.Add(carPart);
                }

                validCars.Add(car);
            }

            context.Cars.AddRange(validCars);
            context.PartCars.AddRange(validCarParts);
            context.SaveChanges();

            return $"Successfully imported {validCars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {

            XmlRootAttribute root = new XmlRootAttribute("Customers");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportCustomerDto>), root);

            var importedCustomers = new List<ImportCustomerDto>();

            using (var reader = new StringReader(inputXml))
            {
                importedCustomers = (List<ImportCustomerDto>)serializer.Deserialize(reader);
            }

            var validCustomers = new List<Customer>();

            foreach (var customerDto in importedCustomers)
            {
                var parseBoolean = Boolean.TryParse(customerDto.isYoungDriver, out bool result);
                if (!parseBoolean)
                {
                    continue;
                }

                var parsedDate = DateTime.Parse(customerDto.Birthdate, CultureInfo.InvariantCulture);

                var customer = new Customer()
                {
                    Name = customerDto.Name,
                    BirthDate = parsedDate,
                    IsYoungDriver = result
                };

                validCustomers.Add(customer);
            }

            context.Customers.AddRange(validCustomers);
            context.SaveChanges();

            return $"Successfully imported {validCustomers.Count}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Sales");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportSaleDto>), root);

            var importedSales = new List<ImportSaleDto>();

            using (var reader = new StringReader(inputXml))
            {
                importedSales = (List<ImportSaleDto>)serializer.Deserialize(reader);
            }

            var validSales = new List<Sale>();

            foreach (var saleDto in importedSales)
            {
                var car = context.Cars
                    .FirstOrDefault(c => c.Id == saleDto.CarId);
                if (car == null)
                {
                    continue;
                }

                var sale = new Sale()
                {
                    Car = car,
                    CustomerId = saleDto.CustomerId,
                    Discount = saleDto.Discount
                };
                validSales.Add(sale);
            }
            context.Sales.AddRange(validSales);
            context.SaveChanges();

            return $"Successfully imported {validSales.Count}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .ToArray()
                .Where(c => c.TravelledDistance > 2000000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportCarDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TravelledDistance
                })
                .ToArray();


            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("cars");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer,cars, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                .ToArray()
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new ExportBmwCarDto()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TravelledDistance
                })
                .ToArray();



            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("cars");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportBmwCarDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, cars, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .ToArray()
                .Where(s => s.IsImporter == false)
                .Select(s => new ExportSupplierDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("suppliers");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportSupplierDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, suppliers, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .ToArray()
                .Select(c => new ExportCarWithPartsDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TravelledDistance,
                    Parts = c.PartCars.Select(pc => new ExportPartForCarDto()
                        {
                            Name = pc.Part.Name,
                            Price = pc.Part.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                })
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .ToArray();


            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("cars");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCarWithPartsDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, cars, namespaces);
            }

            return sb.ToString().TrimEnd();
        }


        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .ToArray()
                .Where(c => c.Sales.Any())
                .Select(c => new ExportCustomerDto()
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count,
                    SpentMoney = c.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price))
                })
                .OrderByDescending(c => c.SpentMoney)
                .ToArray();



            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("customers");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCustomerDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, customers, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context
                .Sales
                .Select(s => new ExportSaleDto()
                {
                    Car = new ExportCarDataWithDiscount()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(pc => pc.Part.Price),
                    PriceWithDiscount = s.Car.PartCars.Sum(pc => pc.Part.Price)
                                        - s.Discount * (s.Car.PartCars.Sum(pc => pc.Part.Price)) / 100
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("sales");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportSaleDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, sales, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}