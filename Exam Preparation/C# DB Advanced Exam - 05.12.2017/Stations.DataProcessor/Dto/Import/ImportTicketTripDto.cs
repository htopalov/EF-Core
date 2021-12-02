using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Stations.DataProcessor.Dto.Import
{

    [XmlRoot("Trip")]
    public class ImportTicketTripDto
    {
        [Required]
        [XmlElement("OriginStation")]
        [MaxLength(50)]
        public string OriginStation { get; set; }
        [Required]
        [XmlElement("DestinationStation")]
        [MaxLength(50)]
        public string DestinationStation { get; set; }
        [Required]
        [XmlElement("DepartureTime")]
        public string DepartureTime { get; set; }
    }
}
