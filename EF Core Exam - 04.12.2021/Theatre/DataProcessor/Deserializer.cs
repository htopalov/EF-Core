using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Theatre.Data.Models;
using Theatre.Data.Models.Enums;
using Theatre.DataProcessor.ImportDto;

namespace Theatre.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Theatre.Data;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";

        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Plays");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportedPlayDto>), root);

            var importedPlays = new List<ImportedPlayDto>();

            using (var reader = new StringReader(xmlString))
            {
                importedPlays = (List<ImportedPlayDto>) serializer.Deserialize(reader);
            }

            var validPlays = new List<Play>();

            foreach (var playDto in importedPlays)
            {
                if (!IsValid(playDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isDurationValid = TimeSpan.TryParseExact(playDto.Duration, "c", CultureInfo.InvariantCulture,
                    TimeSpanStyles.None, out TimeSpan validDuration);

                if (!isDurationValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (validDuration.Hours < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isValidType = Enum.TryParse(typeof(Genre), playDto.Genre, out object validPlayType);
                if (!isValidType)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var play = new Play()
                {
                    Title = playDto.Title,
                    Duration = validDuration,
                    Rating = playDto.Rating,
                    Genre = (Genre)validPlayType,
                    Description = playDto.Description,
                    Screenwriter = playDto.Screenwriter
                };

                validPlays.Add(play);
                sb.AppendLine(string.Format(SuccessfulImportPlay, play.Title, play.Genre.ToString(), play.Rating));
            }

            context.Plays.AddRange(validPlays);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Casts");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportedCastDto>), root);

            var importedCasts = new List<ImportedCastDto>();

            using (var reader = new StringReader(xmlString))
            {
                importedCasts = (List<ImportedCastDto>)serializer.Deserialize(reader);
            }

            var validCasts = new List<Cast>();

            foreach (var castDto in importedCasts)
            {
                if (!IsValid(castDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isBoolValid = Boolean.TryParse(castDto.IsMainCharacter, out bool validBoolean);

                if (!isBoolValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var cast = new Cast()
                {
                    FullName = castDto.FullName,
                    IsMainCharacter = validBoolean,
                    PhoneNumber = castDto.PhoneNumber,
                    PlayId = castDto.PlayId
                };
                validCasts.Add(cast);
                sb.AppendLine(string.Format(SuccessfulImportActor, cast.FullName,
                    cast.IsMainCharacter ? "main" : "lesser"));
            }
            context.Casts.AddRange(validCasts);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            //StringBuilder sb = new StringBuilder();
            //var importedTheaters = JsonConvert.DeserializeObject<ImportTheaterTicketDto[]>(jsonString);

            //var validTheaters = new List<Data.Models.Theatre>();
            //foreach (var theaterDto in importedTheaters)
            //{
            //    if (!IsValid(theaterDto))
            //    {
            //        sb.AppendLine(ErrorMessage);
            //        continue;
            //    }

            //    var validTickets = new List<Ticket>();

            //    var theater = new Data.Models.Theatre()
            //    {
            //        Name = theaterDto.Name,
            //        NumberOfHalls = theaterDto.NumberOfHalls,
            //        Director = theaterDto.Director,
            //        Tickets = validTickets
            //    };

            //    foreach (var ticketDto in theaterDto.Tickets)
            //    {
            //        if (!IsValid(ticketDto))
            //        {
            //            sb.AppendLine(ErrorMessage);
            //            continue;
            //        }

            //        var play = context.Plays.FirstOrDefault(p => p.Id == ticketDto.PlayId);
            //        if (play == null)
            //        {
            //            sb.AppendLine(ErrorMessage);
            //            continue;
            //        }

            //        var ticket = new Ticket()
            //        {
            //            Play = play,
            //            Price = ticketDto.Price,
            //            RowNumber = ticketDto.RowNumber,
            //            Theatre = theater
            //        };
            //        validTickets.Add(ticket);
            //    }

          

            //    validTheaters.Add(theater);
            //    sb.AppendLine(string.Format(SuccessfulImportTheatre, theater.Name, theater.Tickets.Count));
            //}

            //context.Theatres.AddRange(validTheaters);
            //context.SaveChanges();

            //return sb.ToString().TrimEnd();

            StringBuilder sb = new StringBuilder();
            var theatres = JsonConvert.DeserializeObject<TheatreImportDto[]>(jsonString);

            foreach (var theatreDto in theatres)
            {
                if (!IsValid(theatreDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var theatre = new Data.Models.Theatre()
                {
                    Name = theatreDto.Name,
                    NumberOfHalls = theatreDto.NumberOfHalls,
                    Director = theatreDto.Director
                };

                context.Add(theatre);
                context.SaveChanges();

                var tickets = new List<Ticket>();

                foreach (var ticketDto in theatreDto.Tickets)
                {
                    if (!IsValid(ticketDto) || ticketDto.Price < 1 || ticketDto.Price > 100)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var ticket = new Ticket()
                    {
                        Price = ticketDto.Price,
                        RowNumber = ticketDto.RowNumber,
                        PlayId = ticketDto.PlayId,
                        TheatreId = theatre.Id,
                    };

                    tickets.Add(ticket);
                }

                context.AddRange(tickets);
                context.SaveChanges();

                sb.AppendLine(string.Format(SuccessfulImportTheatre, theatre.Name, tickets.Count));
            }

            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
