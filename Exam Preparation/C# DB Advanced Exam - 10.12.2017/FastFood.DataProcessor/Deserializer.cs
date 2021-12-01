using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FastFood.Data;
using FastFood.DataProcessor.Dto.Import;
using FastFood.Models;
using FastFood.Models.Enums;
using Newtonsoft.Json;

namespace FastFood.DataProcessor
{
	public static class Deserializer
	{
		private const string FailureMessage = "Invalid data format.";
		private const string SuccessMessage = "Record {0} successfully imported.";

		public static string ImportEmployees(FastFoodDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var importedEmployees = JsonConvert.DeserializeObject<List<ImportEmployeeDto>>(jsonString);

            var validEmployees = new List<Employee>();

            foreach (var employeeDto in importedEmployees)
            {
                if (!IsValid(employeeDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var position = context.Positions.FirstOrDefault(p => p.Name == employeeDto.Position);
                if (position == null)
                {
                    position = new Position()
                    {
                        Name = employeeDto.Position
                    };
                    context.Positions.Add(position);
                    context.SaveChanges();
                }

                var employee = new Employee()
                {
                    Name = employeeDto.Name,
                    Age = employeeDto.Age,
                    Position = position
                };

                validEmployees.Add(employee);
                sb.AppendLine(String.Format(SuccessMessage, employee.Name));
            }

            context.Employees.AddRange(validEmployees);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

		public static string ImportItems(FastFoodDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedItems = JsonConvert.DeserializeObject<List<ImportItemDto>>(jsonString);
            var validItems = new List<Item>();

            foreach (var itemDto in importedItems)
            {
                if (!IsValid(itemDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isCategoryExisting = context.Categories.Any(c => c.Name == itemDto.Category);

                if (!isCategoryExisting)
                {
                    var category = new Category()
                    {
                        Name = itemDto.Category
                    };

                    context.Categories.Add(category);
                    context.SaveChanges();
                }

                var isItemExisting = validItems.Any(i => i.Name == itemDto.Name);

                if (isItemExisting)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var item = new Item()
                {
                    Name = itemDto.Name,
                    Price = itemDto.Price,
                    Category = context.Categories.Single(c => c.Name == itemDto.Category)
                };

                validItems.Add(item);
                sb.AppendLine(string.Format(SuccessMessage, itemDto.Name));
            }

            context.Items.AddRange(validItems);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

		public static string ImportOrders(FastFoodDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Orders");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportOrderDto>),root);
            var importedOrders = new List<ImportOrderDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedOrders = (List<ImportOrderDto>) serializer.Deserialize(reader);
            }

            var validOrders = new List<Order>();

            foreach (var orderDto in importedOrders)
            {
                if (!IsValid(orderDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isEmployeeExisting = context.Employees.Any(e => e.Name == orderDto.Employee);
                if (!isEmployeeExisting)
                {
                    continue;
                }

                var orderedItems = orderDto.Items.Select(i=>i.Name);
                var isItemAvailable = true;
                foreach (var itemName in orderedItems)
                {
                    if (!context.Items.Any(i=>i.Name == itemName))
                    {
                        isItemAvailable = false;
                        break;
                    }
                }

                if (!isItemAvailable)
                {
                    continue;
                }

                var isOrderDateValid = DateTime.TryParseExact(orderDto.DateTime, "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validOrderDate);

                var employee = context.Employees.FirstOrDefault(e => e.Name == orderDto.Employee);

                var orderType = Enum.TryParse(typeof(OrderType), orderDto.Type, out object validOrderType);

                decimal orderTotalPrice = 0;

                foreach (var name in orderedItems)
                {
                    var itemPrice = context.Items.FirstOrDefault(i => i.Name == name).Price * orderDto.Items.FirstOrDefault(dto=> dto.Name == name).Quantity;
                    orderTotalPrice += itemPrice;
                }

                var order = new Order()
                {
                    Customer = orderDto.Customer,
                    DateTime = validOrderDate,
                    Employee = employee,
                    Type = (OrderType) validOrderType,
                    TotalPrice = orderTotalPrice
                };

                foreach (var item in orderDto.Items)
                {
                    var orderItem = new OrderItem()
                    {
                        Order = order,
                        Item = context.Items.FirstOrDefault(i => i.Name == item.Name),
                        Quantity = orderDto.Items.FirstOrDefault(dto=>dto.Name == item.Name).Quantity
                    };
                    order.OrderItems.Add(orderItem);
                }

                validOrders.Add(order);
                sb.AppendLine($"Order for {order.Customer} on {order.DateTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} added");
            }

            context.Orders.AddRange(validOrders);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
	}
}