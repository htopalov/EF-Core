using System;
using System.Linq;
using System.Text;

namespace BookShop
{
    using Data;
    using Initializer;
    using System.Globalization;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            //string command = Console.ReadLine();
            //Console.WriteLine(GetBooksByAgeRestriction(db,command));
            //Console.WriteLine(GetGoldenBooks(db));
            //Console.WriteLine(GetBooksByPrice(db));
            //int year = int.Parse(Console.ReadLine());
            //Console.WriteLine(GetBooksNotReleasedIn(db,year));
            //var categories = Console.ReadLine();
            //Console.WriteLine(GetBooksByCategory(db,categories));
            //string date = Console.ReadLine();
            //Console.WriteLine(GetBooksReleasedBefore(db,date));
            //string ending = Console.ReadLine();
            //Console.WriteLine(GetAuthorNamesEndingIn(db, ending));
            //string input = Console.ReadLine();
            //Console.WriteLine(GetBookTitlesContaining(db, input));
            //string input = Console.ReadLine();
            //Console.WriteLine(GetBooksByAuthor(db, input));
            //int length = int.Parse(Console.ReadLine());
            //Console.WriteLine(CountBooks(db, length));
            //Console.WriteLine(CountCopiesByAuthor(db));
        }


        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var books = context.Books
                .Select(b => new
                {
                    b.Title,
                    b.AgeRestriction
                })
                .OrderBy(b => b.Title)
                .ToList();
            var sb = new StringBuilder();
            foreach (var book in books.Where(b => b.AgeRestriction.ToString().ToLower() == command.ToLower()))
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }


        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Copies,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToList();
            var sb = new StringBuilder();
            foreach (var book in books.Where(b=> b.EditionType.ToString() == "Gold" && b.Copies < 5000))
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context
                .Books
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .ToList();
            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Select(b => new
                {
                    b.Title,
                    b.ReleaseDate,
                    b.BookId
                })
                .Where(b=> b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .ToList();

            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(s=>s.ToLower());

            var books = context.BooksCategories
                .Select(b => new
                {
                    b.Category.Name,
                    b.Book.Title
                })
                .Where(b => categories.Contains(b.Name.ToLower()))
                .OrderBy(b => b.Title)
                .ToList();
            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var books = context.Books
                .Select(b => new
                {
                    Title = b.Title,
                    EditionType = b.EditionType.ToString(),
                    Price = b.Price,
                    ReleaseDate = b.ReleaseDate
                })
                .Where(b => b.ReleaseDate.Value < DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();
            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .ToList()
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    Fullname = $"{a.FirstName} {a.LastName}"
                    //a.FirstName,
                    //a.LastName
                })
                .OrderBy(a=>a.Fullname)
                .ToList();

            var sb = new StringBuilder();
            foreach (var author in authors)
            {
                sb.AppendLine(author.Fullname);
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .ToList()
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => new
                {
                    b.Title
                })
                .OrderBy(b => b.Title)
                .ToList();

            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .ToList()
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()) && b.Author.FirstName != null)
                .Select(b => new
                {
                    b.Title,
                    b.Author.FirstName,
                    b.Author.LastName,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToList();

            var sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.FirstName} {book.LastName})");
            }

            return sb.ToString().TrimEnd();

        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var booksCount = context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .Count();

            return booksCount;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
               .Select(a => new
               {
                   FirstName = a.FirstName,
                   LastName = a.LastName,
                   Count = a.Books.Sum(b => b.Copies)
               })
               .OrderByDescending(a => a.Count)
               .ToList();

            var sb = new StringBuilder();
            foreach (var author in authors)
            {
                sb.AppendLine($"{author.FirstName} {author.LastName} - {author.Count}");
            }

            return sb.ToString().TrimEnd();
                
        }
    }
}
