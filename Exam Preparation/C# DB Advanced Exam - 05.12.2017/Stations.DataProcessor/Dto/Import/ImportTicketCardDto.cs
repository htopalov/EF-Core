using System.Xml.Serialization;

namespace Stations.DataProcessor.Dto.Import
{
    [XmlRoot("Card")]
    public class ImportTicketCardDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}
