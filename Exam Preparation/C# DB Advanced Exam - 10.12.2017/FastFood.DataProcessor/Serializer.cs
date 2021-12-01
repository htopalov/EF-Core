using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FastFood.Data;
using FastFood.DataProcessor.Dto.Export;
using FastFood.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FastFood.DataProcessor
{
    public class Serializer
    {
        public static string ExportOrdersByEmployee(FastFoodDbContext context, string employeeName, string orderType)
        {
            OrderType type = Enum.Parse<OrderType>(orderType);

            var employee = context
                .Employees
                .Where(e => e.Name == employeeName)
                .Select(e => new
                {
                    e.Name,
                    Orders = e.Orders.Where(o => o.Type == type).Select(o => new
                        {
                            o.Customer,
                            Items = o.OrderItems.Select(oi => new
                            {
                                oi.Item.Name,
                                oi.Item.Price,
                                oi.Quantity
                            }).ToArray(),
                            TotalPrice = o.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity)
                        })
                        .OrderByDescending(o => o.TotalPrice)
                        .ThenByDescending(o => o.Items.Length)
                        .ToArray(),
                    TotalMade = e.Orders
                        .Where(o => o.Type == type)
                        .Sum(o => o.OrderItems.Sum(oi => oi.Item.Price * oi.Quantity))
                })
                .SingleOrDefault();

            return JsonConvert.SerializeObject(employee, Formatting.Indented);

        }

        public static string ExportCategoryStatistics(FastFoodDbContext context, string categoriesString)
        {
            string[] searchedCategories = categoriesString.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            CategoryExportDto[] categories = context
                .Categories
                .Include(c => c.Items)
                .Where(c => searchedCategories.Contains(c.Name))
                .Select(c => new CategoryExportDto()
                {
                    Name = c.Name,
                    MostPopularItem = c.Items.Select(i => new MostPopularItemDto()
                        {
                            Name = i.Name,
                            TotalMade = i.OrderItems.Sum(oi => oi.Quantity * oi.Item.Price),
                            TimesSold = i.OrderItems.Sum(oi => oi.Quantity)
                        })
                        .OrderByDescending(i => i.TotalMade)
                        .ThenByDescending(i => i.TimesSold)
                        .First()
                })
                .OrderByDescending(c => c.MostPopularItem.TotalMade)
                .ThenByDescending(c => c.MostPopularItem.TimesSold)
                .ToArray();




            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Categories");
            XmlSerializer serializer = new XmlSerializer(typeof(CategoryExportDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer,categories,namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}