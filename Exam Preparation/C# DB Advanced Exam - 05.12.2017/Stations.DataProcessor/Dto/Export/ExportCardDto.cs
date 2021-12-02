using System.Collections.Generic;
using System.Xml.Serialization;
using Stations.Models.Enums;

namespace Stations.DataProcessor.Dto.Export
{
    [XmlType("Card")]
    public class ExportCardDto
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("type")]
        public CardType type { get; set; }

        [XmlArray("Tickets"), XmlArrayItem("Ticket")]
        public List<ExportTicketDto> Tickets { get; set; }
    }
}
