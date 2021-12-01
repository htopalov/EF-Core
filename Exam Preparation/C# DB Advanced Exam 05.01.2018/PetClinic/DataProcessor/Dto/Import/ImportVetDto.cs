using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace PetClinic.DataProcessor.Dto.Import
{
    [XmlType("Vet")]
    public class ImportVetDto
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(3)]
        [MaxLength(40)]
        public string Name { get; set; }

        [XmlElement("Profession")]
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Profession { get; set; }

        [XmlElement("Age")]
        [Range(22,65)]
        public int Age { get; set; }


        [XmlElement("PhoneNumber")]
        [Required]
        [RegularExpression(@"(^[0-9]{10})|(\+359[0-9]{9})$")]
        public string PhoneNumber { get; set; }
    }
}
