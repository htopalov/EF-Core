using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CarDealer.Dto.Import
{
    [XmlType("Supplier")]
    public class ImportSupplierDto
    {
        [XmlElement("name")]
        [Required]
        public string Name { get; set; }

        [XmlElement("isImporter")]
        public string isImporter { get; set; }
    }
}
