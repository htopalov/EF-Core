using System.Xml.Serialization;

namespace CarDealer.Dto.Import
{
    [XmlType("partId")]
    public class ImportPartCarDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
