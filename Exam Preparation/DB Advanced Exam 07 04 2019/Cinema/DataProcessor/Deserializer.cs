using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Cinema.Data.Models;
using Cinema.Data.Models.Enums;
using Cinema.DataProcessor.ImportDto;
using Newtonsoft.Json;

namespace Cinema.DataProcessor
{
    using System;

    using Data;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            // no dto for movie
            var desMovies = JsonConvert.DeserializeObject<Movie[]>(jsonString);

            var validMovies = new List<Movie>();

            var sb = new StringBuilder();

            foreach (var m in desMovies)
            {
                var isValidEnum = Enum.TryParse(typeof(Genre), m.Genre.ToString(), out object result);
                var movieExists = validMovies.Any(t => t.Title == m.Title);

                if (!IsValid(m) || !isValidEnum || movieExists)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = new Movie { Title = m.Title, Genre = m.Genre, Duration = m.Duration, Rating = m.Rating, Director = m.Director };
                validMovies.Add(movie);

                sb.AppendLine(String.Format(SuccessfulImportMovie, movie.Title, movie.Genre, movie.Rating.ToString("F2")));
            }

            context.AddRange(validMovies);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedHallSeats = JsonConvert.DeserializeObject<ImportHallSeatsDto[]>(jsonString);
            var validHalls = new List<Hall>();
            foreach (var hallDto in importedHallSeats)
            {
                if (!IsValid(hallDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hall = new Hall()
                {
                    Name = hallDto.Name,
                    Is4Dx = hallDto.Is4Dx,
                    Is3D = hallDto.Is3D
                };

                for (int i = 0; i < hallDto.Seats; i++)
                {
                    hall.Seats.Add(new Seat());
                }
                validHalls.Add(hall);
                var status = string.Empty;

                if (hall.Is4Dx && hall.Is3D)
                {
                    status = "4Dx/3D";
                }
                else if (hall.Is4Dx && !hall.Is3D)
                {
                    status = "4Dx";
                }
                else if (!hall.Is4Dx && hall.Is3D)
                {
                    status = "3D";
                }
                else
                {
                    status = "Normal";
                }

                sb.AppendLine(String.Format(SuccessfulImportHallSeat, hall.Name, status, hall.Seats.Count));
            }
            context.AddRange(validHalls);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Projections");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportProjectionDto>), root);
            var importedProjections = new List<ImportProjectionDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedProjections = (List<ImportProjectionDto>)serializer.Deserialize(reader);
            }

            var validProjections = new List<Projection>();
            foreach (var projectionDto in importedProjections)
            {
                var movie = context.Movies.Find(projectionDto.MovieId);
                var hall = context.Halls.Find(projectionDto.HallId);

                if (movie == null || hall == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var projectionDate = DateTime.ParseExact(projectionDto.DateTime, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);

                var projection = new Projection()
                {
                    MovieId = projectionDto.MovieId,
                    HallId = projectionDto.HallId,
                    DateTime = projectionDate
                };
                validProjections.Add(projection);
                sb.AppendLine(string.Format(SuccessfulImportProjection, movie.Title, projection.DateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)));
            }
            context.Projections.AddRange(validProjections);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Customers");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportCustomerDto>), root);
            var importedCustomers = new List<ImportCustomerDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedCustomers = (List<ImportCustomerDto>)serializer.Deserialize(reader);
            }

            var validCustomers = new List<Customer>();

            foreach (var customerDto in importedCustomers)
            {
                if (!IsValid(customerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var validTickets = new List<Ticket>();

                foreach (var ticketDto in customerDto.Tickets)
                {
                    if (!IsValid(ticketDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var ticket = new Ticket()
                    {
                        ProjectionId = ticketDto.ProjectionId,
                        Price = ticketDto.Price
                    };

                    validTickets.Add(ticket);
                }

                if (validTickets.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var customer = new Customer()
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Age = customerDto.Age,
                    Balance = customerDto.Balance
                };

                customer.Tickets = validTickets;
                validCustomers.Add(customer);
                sb.AppendLine(String.Format(SuccessfulImportCustomerTicket, customer.FirstName, customer.LastName,
                    customer.Tickets.Count));
            }

            context.Customers.AddRange(validCustomers);
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