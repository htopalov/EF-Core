using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Stations.DataProcessor.Dto.Import
{
    [XmlType("Ticket")]
    public class ImportTicketDto
    {
        [XmlAttribute("price")]
        [Required]
        [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [XmlAttribute("seat")]
        [Required]
        [MaxLength(8)]
        [RegularExpression(@"^[A-Za-z]{2}[0-9]{1,6}$")]
        public string Seat { get; set; }

        [XmlElement("Trip")]
        public ImportTicketTripDto Trip { get; set; }

        [XmlElement("Card")]
        public ImportTicketCardDto Card { get; set; } = new ImportTicketCardDto();
    }
}
