using BookShop.Data.Models;
using BookShop.Data.Models.Enums;
using BookShop.DataProcessor.ImportDto;

namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportBookDto>), new XmlRootAttribute("Books"));
            var importedBooks = new List<ImportBookDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedBooks = (List<ImportBookDto>)serializer.Deserialize(reader);
            }

            var validBooks = new List<Book>();

            foreach (var bookDto in importedBooks)
            {
                if (!IsValid(bookDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isPublishedOnDateValid = DateTime.TryParseExact(bookDto.PublishedOn, "MM/dd/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validPublishedOnDate);

                if (!isPublishedOnDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var book = new Book()
                {
                    Name = bookDto.Name,
                    Genre = (Genre)bookDto.Genre,
                    Price = bookDto.Price,
                    Pages = bookDto.Pages,
                    PublishedOn = validPublishedOnDate
                };

                validBooks.Add(book);
                sb.AppendLine(String.Format(SuccessfullyImportedBook, book.Name, book.Price));
            }

            context.Books.AddRange(validBooks);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedAuthors = JsonConvert.DeserializeObject<ImportAuthorDto[]>(jsonString);

            var validAuthors = new List<Author>(); ;

            foreach (var authorDto in importedAuthors)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hasEmail = validAuthors.FirstOrDefault(a => a.Email == authorDto.Email);
                if (hasEmail != null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var author = new Author()
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Phone = authorDto.Phone,
                    Email = authorDto.Email,
                };

                foreach (var bookDto in authorDto.Books.Distinct())
                {
                    var dbBook = context.Books.FirstOrDefault(b => b.Id == bookDto.Id);
                    if (dbBook == null)
                    {
                        continue;
                    }

                    author.AuthorsBooks.Add(new AuthorBook
                    {
                        Author = author,
                        Book = dbBook
                    });
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                validAuthors.Add(author);
                sb.AppendLine(string.Format(SuccessfullyImportedAuthor, (author.FirstName + " " + author.LastName), author.AuthorsBooks.Count));
            }

            context.Authors.AddRange(validAuthors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}

