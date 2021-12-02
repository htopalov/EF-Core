using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stations.Data;
using Stations.DataProcessor.Dto.Export;
using Stations.Models.Enums;

namespace Stations.DataProcessor
{
	public class Serializer
	{
        public static string ExportDelayedTrains(StationsDbContext context, string dateAsString)
        {
            var date = DateTime.ParseExact(dateAsString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var delayedTrains = context.Trains
                .Include(e => e.Trips)
                .Where(e => e.Trips.Any(t => t.Status == Enum.Parse<TripStatus>("Delayed") && t.DepartureTime.Ticks <= date.Ticks))
                .Select(e => new
                {
                    TrainNumber = e.TrainNumber,
                    DelayedTrips = e.Trips
                        .Where(t => t.Status == Enum.Parse<TripStatus>("Delayed") && t.DepartureTime.Ticks <= date.Ticks)
                        .ToArray(),
                }).Select(t => new
                {
                    TrainNumber = t.TrainNumber,
                    DelayedTimes = t.DelayedTrips.Count(),
                    MaxDelayedTime = t.DelayedTrips.Max(s => s.TimeDifference).ToString()
                })
                .OrderByDescending(t => t.DelayedTimes)
                .ThenByDescending(t => t.MaxDelayedTime)
                .ThenBy(t => t.TrainNumber)
                .ToArray();

            return JsonConvert.SerializeObject(delayedTrains, Newtonsoft.Json.Formatting.Indented);
        }

		public static string ExportCardsTicket(StationsDbContext context, string cardType)
		{
            var ticketsByCardType = context.CustomerCards
                .Include(e => e.BoughtTickets)
                .ThenInclude(e => e.Trip)
                .Where(e => e.Type == Enum.Parse<CardType>(cardType))
                .Select(c => new ExportCardDto()
                {
                    Name = c.Name,
                    type = c.Type,

                    Tickets = c.BoughtTickets.Select(t => new ExportTicketDto()
                    {
                        DepartureTime = t.Trip.DepartureTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                        DestinationStation = t.Trip.DestinationStation.Name,
                        OriginStation = t.Trip.OriginStation.Name
                    }).ToList()

                })
                .OrderBy(e => e.Name)
                .Where(e => e.Tickets.Count != 0)
                .ToArray();

            var xmlNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCardDto[]), new XmlRootAttribute("Cards"));
            var sb = new StringBuilder();
            serializer.Serialize(new StringWriter(sb), ticketsByCardType, xmlNamespaces);


            return sb.ToString();
        }
    }
}