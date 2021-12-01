using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace PetClinic.DataProcessor.Dto.Import
{
    [XmlType("Procedure")]
    public class ImportProcedureDto
    {
        [XmlElement("Vet")]
        [Required]
        [MinLength(3)]
        [MaxLength(40)]
        public string VetName { get; set; }

        [XmlElement("Animal")]
        [Required]
        [RegularExpression(@"^[a-zA-Z]{7}[0-9]{3}$")]
        public string AnimalPassportSerialNumber { get; set; }

        [XmlElement("DateTime")]
        [Required]
        public string DateTime { get; set; }

        [XmlArray("AnimalAids")]
        public ImportAnimalAidXmlDto[] AnimalAids { get; set; }
    }
}

