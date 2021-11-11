using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.DTO;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            Mapper.Initialize(cfg => cfg.AddProfile(new ProductShopProfile()));
            var db = new ProductShopContext();
            //var input = File.ReadAllText("../../../Datasets/users.json");
            //Console.WriteLine(ImportUsers(db, input));
            //var input = File.ReadAllText("../../../Datasets/products.json");
            //Console.WriteLine(ImportProducts(db, input));
            //var input = File.ReadAllText("../../../Datasets/categories.json");
            //Console.WriteLine(ImportCategories(db, input));
            //var input = File.ReadAllText("../../../Datasets/categories-products.json");
            //Console.WriteLine(ImportCategoryProducts(db, input));
            //File.WriteAllText("../../../Results/products-in-range.json", GetProductsInRange(db));
            //File.WriteAllText("../../../Results/users-sold-products.json", GetSoldProducts(db));
            //File.WriteAllText("../../../Results/categories-by-products.json", GetCategoriesByProductsCount(db));
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<List<User>>(inputJson);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(inputJson);

            context.Products.AddRange(products);
            context.SaveChanges();
            return $"Successfully imported {products.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<List<Category>>(inputJson)
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToList();
            context.Categories.AddRange(categories);
            context.SaveChanges();
            return $"Successfully imported {categories.Count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var cp = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson);

            context.CategoryProducts.AddRange(cp);
            context.SaveChanges();
            return $"Successfully imported {cp.Count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            List<ProductsInRange> products = context.Products
                .OrderBy(x => x.Price)
                .Where(x => x.Price >= 500M && x.Price <= 1000M)
                .ProjectTo<ProductsInRange>()
                .ToList();

            return JsonConvert.SerializeObject(products, Formatting.Indented);

        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(ps => ps.ProductsSold)
                .Where(u => u.ProductsSold.Any(b => b.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ProjectTo<UserSoldProducts>()
                .ToList();

            return JsonConvert.SerializeObject(users, Formatting.Indented);
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(c => c.CategoryProducts.Count)
                .Select(x => new
                {
                    Category = x.Name,
                    ProductsCount = x.CategoryProducts.Count,
                    AveragePrice = $"{x.CategoryProducts.Average(c => c.Product.Price):F2}",
                    TotalRevenue = $"{x.CategoryProducts.Sum(c => c.Product.Price)}"
                })
                .ToList();

            string json = JsonConvert.SerializeObject(categories,
                new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy(),
                    },

                    Formatting = Formatting.Indented
                }
            );

            return json;
        }
    }
}