using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stations.Data;
using Stations.DataProcessor.Dto.Import;
using Stations.Models;
using Stations.Models.Enums;

namespace Stations.DataProcessor
{
    public static class Deserializer
    {
        private const string FailureMessage = "Invalid data format.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportStations(StationsDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedStations = JsonConvert.DeserializeObject<List<ImportStationDto>>(jsonString);

            var validStations = new List<Station>();

            foreach (var stationDto in importedStations)
            {
                if (!IsValid(stationDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (stationDto.Town == null)
                {
                    stationDto.Town = stationDto.Name;
                }

                var isStationExisting = validStations.Any(s => s.Name == stationDto.Name);
                if (isStationExisting)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var station = new Station()
                {
                    Name = stationDto.Name,
                    Town = stationDto.Town
                };
                validStations.Add(station);
                sb.AppendLine(String.Format(SuccessMessage, station.Name));
            }
            context.Stations.AddRange(validStations);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportClasses(StationsDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedClasses = JsonConvert.DeserializeObject<List<ImportSeatingClassDto>>(jsonString);

            var validSeatingClasses = new List<SeatingClass>();

            foreach (var classDto in importedClasses)
            {
                if (!IsValid(classDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isClassExisting = validSeatingClasses.Any(c =>
                    c.Name == classDto.Name || c.Abbreviation == classDto.Abbreviation);
                if (isClassExisting)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var seatingClass = new SeatingClass()
                {
                    Name = classDto.Name,
                    Abbreviation = classDto.Abbreviation
                };
                validSeatingClasses.Add(seatingClass);
                sb.AppendLine(String.Format(SuccessMessage, seatingClass.Name));
            }
            context.SeatingClasses.AddRange(validSeatingClasses);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportTrains(StationsDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedTrains = JsonConvert.DeserializeObject<ImportTrainDto[]>(jsonString, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var validTrains = new List<Train>();

            foreach (var trainDto in importedTrains)
            {
                if (!IsValid(trainDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isTrainExisting = validTrains.Any(t => t.TrainNumber == trainDto.TrainNumber);
                if (isTrainExisting)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var areAllSeatsValid = trainDto.Seats.All(IsValid);
                if (!areAllSeatsValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var areSeatingClassesValid = trainDto.Seats
                    .All(s => context.SeatingClasses.Any(sc => sc.Name == s.Name && sc.Abbreviation == s.Abbreviation));
                if (!areSeatingClassesValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var type = Enum.Parse<TrainType>(trainDto.Type);

                var trainSeats = trainDto.Seats.Select(s => new TrainSeat
                {
                    SeatingClass =
                            context.SeatingClasses.SingleOrDefault(sc => sc.Name == s.Name && sc.Abbreviation == s.Abbreviation),
                    Quantity = s.Quantity.Value
                })
                    .ToArray();

                var train = new Train
                {
                    TrainNumber = trainDto.TrainNumber,
                    Type = type,
                    TrainSeats = trainSeats
                };

                validTrains.Add(train);

                sb.AppendLine(string.Format(SuccessMessage, trainDto.TrainNumber));
            }

            context.Trains.AddRange(validTrains);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTrips(StationsDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var importedTrips = JsonConvert.DeserializeObject<ImportTripDto[]>(jsonString, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            });

            var validTrips = new List<Trip>();

            foreach (var tripDto in importedTrips)
            {
                var train = context.Trains.FirstOrDefault(e => e.TrainNumber == tripDto.Train);
                if (train == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }
                if (!IsValid(tripDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var originStation = context.Stations.FirstOrDefault(e => e.Name == tripDto.OriginStation);
                if (originStation == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var destinationStation = context.Stations.FirstOrDefault(e => e.Name == tripDto.DestinationStation);
                if (destinationStation == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (!DateTime.TryParseExact(tripDto.DepartureTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validDepartureTime))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (!DateTime.TryParseExact(tripDto.ArrivalTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validArrivalTime))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }
                if (validArrivalTime < validDepartureTime)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var statusAsEnum = Enum.Parse<TripStatus>(tripDto.Status);

                if (!TimeSpan.TryParseExact(tripDto.TimeDifference, @"hh:mm", CultureInfo.InvariantCulture,
                    out TimeSpan validTimespan))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var trip = new Trip()
                {
                    ArrivalTime = validArrivalTime,
                    DepartureTime = validDepartureTime,
                    DestinationStationId = destinationStation.Id,
                    OriginStationId = originStation.Id,
                    TimeDifference = validTimespan,
                    TrainId = train.Id,
                    Status = statusAsEnum
                };
                sb.AppendLine($"Trip from {originStation.Name} to {destinationStation.Name} imported.");
                validTrips.Add(trip);
            }

            context.Trips.AddRange(validTrips);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCards(StationsDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Cards");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportCardDto>), root);
            var importedCards = new List<ImportCardDto>();
            
            using (var reader = new StreamReader(xmlString))
            {
                importedCards = (List<ImportCardDto>) serializer.Deserialize(reader);
            }

            var validCards = new List<CustomerCard>();

            foreach (var cardDto in importedCards)
            {
                if (!IsValid(cardDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (cardDto.CardType == null)
                {
                    cardDto.CardType = "Normal";
                }

                var cardTypeAsEnum = Enum.Parse<CardType>(cardDto.CardType);

                var card = new CustomerCard()
                {
                    Name = cardDto.Name,
                    Age = cardDto.Age,
                    Type = cardTypeAsEnum
                };

                validCards.Add(card);
                sb.AppendLine(string.Format(SuccessMessage, card.Name));
            }

            context.AddRange(validCards);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTickets(StationsDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ImportTicketDto[]), new XmlRootAttribute("Tickets"));
            var deserializedTickets = (ImportTicketDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var validTickets = new List<Ticket>();

            foreach (var item in deserializedTickets)
            {
                var departureTime = DateTime.ParseExact(item.Trip.DepartureTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None);
                var trip = context.Trips
                    .Include(e => e.OriginStation)
                    .Include(e => e.DestinationStation)
                    .Include(e => e.Train)
                    .ThenInclude(e => e.TrainSeats)
                    .SingleOrDefault(e => e.DepartureTime == departureTime && e.DestinationStation.Name == item.Trip.DestinationStation && e.OriginStation.Name == item.Trip.OriginStation);

                if (trip == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }
                if (!IsValid(item))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var searchedAbbreviationArr = item.Seat.Take(2).ToArray();
                var searchedAbbreviation = string.Join("", searchedAbbreviationArr);

                var placeNumber = int.Parse(string.Join("", item.Seat.Skip(2).ToArray()));

                var seatExists = trip.Train.TrainSeats
                    .SingleOrDefault(s => s.SeatingClass.Abbreviation == searchedAbbreviation && placeNumber <= s.Quantity);

                if (seatExists == null)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }
                CustomerCard card = null;
                if (item.Card != null)
                {
                    card = context.CustomerCards.SingleOrDefault(c => c.Name == item.Card.Name);

                    if (card == null)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }
                }
                var ticket = new Ticket()
                {
                    CustomerCard = card,
                    Price = item.Price,
                    Trip = trip,
                    SeatingPlace = item.Seat
                };



                validTickets.Add(ticket);
                sb.AppendLine($"Ticket from {item.Trip.OriginStation} to {item.Trip.DestinationStation}" +
                    $" departing at {departureTime} imported.");
            }
            context.AddRange(validTickets);
            context.SaveChanges();



            return sb.ToString().TrimEnd();
        }


        public static bool IsValid(object obj)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(obj, serviceProvider: null, items: null);

            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(obj, context, results, true);
            return isValid;
        }
    }
}