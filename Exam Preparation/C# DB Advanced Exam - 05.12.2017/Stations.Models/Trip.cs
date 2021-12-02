using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stations.Models.Enums;

namespace Stations.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(OriginStation))]
        public int OriginStationId { get; set; }
        public virtual Station OriginStation { get; set; }

        [ForeignKey(nameof(DestinationStation))]
        public int DestinationStationId { get; set; }
        public virtual Station DestinationStation { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; } //must be after departure time

        [ForeignKey(nameof(Train))]
        public int TrainId { get; set; }
        public virtual Train Train { get; set; }

        public TripStatus Status { get; set; }

        public TimeSpan? TimeDifference { get; set; }
    }
}
