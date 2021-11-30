using BookShop.Data.Models.Enums;
using BookShop.DataProcessor.ExportDto;

namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors
                .ToArray()
                .Select(a => new ExportAuthorDto()
                {
                    AuthorName = $"{a.FirstName} {a.LastName}",
                    Books = a.AuthorsBooks
                        .OrderByDescending(p => p.Book.Price)
                        .Select(b => new ExportAuthorBookDto()
                        {
                            BookName = b.Book.Name,
                            BookPrice = $"{b.Book.Price:f2}"
                        })
                        .ToArray()
                })
                .OrderByDescending(a => a.Books.Length)
                .ThenBy(a => a.AuthorName)
                .ToArray();

            return JsonConvert.SerializeObject(authors, Formatting.Indented);
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .ToArray()
                .Where(b => b.Genre == Genre.Science && b.PublishedOn < date)
                .OrderByDescending(b => b.Pages)
                .ThenByDescending(b => b.PublishedOn)
                .Take(10)
                .Select(b => new ExportBookDto()
                {
                    Name = b.Name,
                    Pages = b.Pages,
                    Date = b.PublishedOn.ToString("d" , CultureInfo.InvariantCulture)
                })
                .ToArray();


            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Books");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportBookDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer,books,namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}