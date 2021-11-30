using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Cinema.DataProcessor.ExportDto;
using Newtonsoft.Json;

namespace Cinema.DataProcessor
{
    using System;

    using Data;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var topMovies = context.Movies
                .ToArray()
                .Where(m => m.Rating >= rating && m.Projections.Any(p => p.Tickets.Count >= 1))
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(p => p.Projections.Sum(t => t.Tickets.Sum(pc => pc.Price)))
                .Select(m => new ExportMovieDto()
                {
                    MovieName = m.Title,
                    Rating = m.Rating.ToString("F2"),
                    TotalIncomes = m.Projections.Sum(d => d.Tickets.Sum(t => t.Price)).ToString("F2"),
                    Customers = m.Projections
                        .SelectMany(c => c.Tickets)
                        .Select(c => new ExportCustomerDto()
                        {
                            FirstName = c.Customer.FirstName,
                            LastName = c.Customer.LastName,
                            Balance = c.Customer.Balance.ToString("F2")
                        })
                        .OrderByDescending(c => c.Balance)
                        .ThenBy(c => c.FirstName)
                        .ThenBy(c => c.LastName)
                        .ToArray()
                })
                .Take(10)
                .ToArray();

            return JsonConvert.SerializeObject(topMovies, Formatting.Indented);
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var customers = context.Customers
                .ToArray()
                .Where(c=> c.Age >= age)
                .Select(c=> new ExportCustomerXmlDto()
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    SpentMoney = c.Tickets.Sum(x=>x.Price).ToString("F2"),
                    SpentTime = TimeSpan.FromSeconds(c.Tickets.Sum(x => x.Projection.Movie.Duration.TotalSeconds)).ToString(@"hh\:mm\:ss")
                })
                .OrderByDescending(p => decimal.Parse(p.SpentMoney))
                .Take(10)
                .ToArray();

            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Customers");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCustomerXmlDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer,customers,namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}