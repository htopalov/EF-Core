using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            //Mapper.Initialize(cfg => cfg.AddProfile<ProductShopProfile>());
            var db = new ProductShopContext();
            //var input = File.ReadAllText("../../../Datasets/users.xml");
            //Console.WriteLine(ImportUsers(db,input));
            //var input = File.ReadAllText("../../../Datasets/products.xml");
            //Console.WriteLine(ImportProducts(db, input));
            //var input = File.ReadAllText("../../../Datasets/categories.xml");
            //Console.WriteLine(ImportCategories(db, input));
            //var input = File.ReadAllText("../../../Datasets/categories-products.xml");
            //Console.WriteLine(ImportCategoryProducts(db, input));
            //File.WriteAllText("../../../Results/products-in-range.xml", GetProductsInRange(db));
            //File.WriteAllText("../../../Results/users-sold-products.xml", GetSoldProducts(db));
            File.WriteAllText("../../../Results/users-and-products.xml", GetUsersWithProducts(db));

        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserImport[]), new XmlRootAttribute("Users"));

            UserImport[] userDtos;

            using (var reader = new StringReader(inputXml))
            {
                userDtos = (UserImport[])serializer.Deserialize(reader);
            }

            var users = Mapper.Map<User[]>(userDtos);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProductImport[]), new XmlRootAttribute("Products"));
            ProductImport[] productImportDtos;
            using (var reader = new StringReader(inputXml))
            {
                productImportDtos = (ProductImport[])serializer.Deserialize(reader);
            }

            var products = new List<Product>();
            foreach (var dto in productImportDtos)
            {
                products.Add(new Product
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    SellerId = dto.SellerId,
                    BuyerId = dto.BuyerId
                });
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<CategoryImport>),
                new XmlRootAttribute("Categories"));

            var categoryDtos = new List<CategoryImport>();

            using (var reader = new StringReader(inputXml))
            {
                categoryDtos = (List<CategoryImport>)serializer.Deserialize(reader);
            }

            var categories = new List<Category>();

            foreach (var dto in categoryDtos)
            {
                if (string.IsNullOrEmpty(dto.Name))
                {
                    continue;
                }

                categories.Add(new Category
                {
                    Name = dto.Name
                });

            }

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<CategoriesProductsImport>),
                new XmlRootAttribute("CategoryProducts"));

            var categoryProductsDtos = new List<CategoriesProductsImport>();

            using (var reader = new StringReader(inputXml))
            {
                categoryProductsDtos = (List<CategoriesProductsImport>)serializer.Deserialize(reader);
            }

            var categoryProducts = new List<CategoryProduct>();

            var categoryIds = context.Categories.Select(x => x.Id).ToList();
            var productIds = context.Products.Select(x => x.Id).ToList();

            foreach (var dto in categoryProductsDtos)
            {
                if (!categoryIds.Contains(dto.CategoryId) || !productIds.Contains(dto.ProductId))
                {
                    continue;
                }

                categoryProducts.Add(new CategoryProduct
                {
                    CategoryId = dto.CategoryId,
                    ProductId = dto.ProductId
                });
            }

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Products");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductInRangeExport[]), xmlRoot);


            using (StringWriter stringWriter = new StringWriter(sb))
            {
                ProductInRangeExport[] products = context.Products
                    .Where(p => p.Price >= 500 && p.Price <= 1000)
                    .OrderBy(o => o.Price)
                    .Take(10)
                    .Select(s => new ProductInRangeExport
                    {
                        Name = s.Name,
                        Price = s.Price,
                        Buyer = $"{s.Buyer.FirstName} {s.Buyer.LastName}" ?? s.Buyer.LastName
                    })
                    .ToArray();

                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(string.Empty, string.Empty);


                xmlSerializer.Serialize(stringWriter, products, xmlSerializerNamespaces);

            }
            return sb.ToString().TrimEnd();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var productToExport = context
                .Users
                .Where(u => u.ProductsSold.Any())
                .Select(p=> new ExportUsersSoldProductsDto()
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    SoldProducts = p.ProductsSold.Select(x=> new ExportSoldProductDTO()
                    {
                        Name = x.Name,
                        Price = x.Price
                    })
                        .ToArray()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<ExportUsersSoldProductsDto>), new XmlRootAttribute("Users"));

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, productToExport, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Categories");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCategoryDto[]), xmlRoot);

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                ExportCategoryDto[] categories = context.Categories
                    .Select(s => new ExportCategoryDto
                    {
                        Name = s.Name,
                        Count = s.CategoryProducts.Count,
                        AveragePrice = s.CategoryProducts.Average(a => a.Product.Price),
                        TotalRevenue = s.CategoryProducts.Sum(v => v.Product.Price)

                    })
                    .OrderByDescending(o => o.Count)
                    .ThenBy(o => o.TotalRevenue)
                    .ToArray();

                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                xmlSerializer.Serialize(stringWriter, categories, namespaces);
            }
            return sb.ToString().TrimEnd();

        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count())
                .Select(u => new ExportUserAndHisProductsDTO
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportSoldProductsDTO
                    {
                        Count = u.ProductsSold.Count,
                        SoldProducts = u.ProductsSold.Select(ps => new ExportSoldProductDTO
                            {
                                Name = ps.Name,
                                Price = ps.Price
                            })
                            .OrderByDescending(sp => sp.Price)
                            .ToList()

                    }
                })
                .ToList();

            var usersToExport = new ExportUsersDTO
            {
                Count = users.Count,
                Users = users.Take(10).ToList()
            };

            var serializer = new XmlSerializer(typeof(ExportUsersDTO), new XmlRootAttribute("Users"));

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, usersToExport, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

    }
}