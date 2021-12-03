using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CarDealer.Dto.Import
{
    [XmlType("Customer")]
    public class ImportCustomerDto
    {
        [XmlElement("name")]
        [Required]
        public string Name { get; set; }

        [XmlElement("birthDate")]
        [Required]
        public string Birthdate { get; set; }

        [XmlElement("isYoungDriver")]
        [Required]
        public string isYoungDriver { get; set; }
    }
}
