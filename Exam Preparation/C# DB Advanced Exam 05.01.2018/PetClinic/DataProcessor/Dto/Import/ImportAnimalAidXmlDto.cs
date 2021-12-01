using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace PetClinic.DataProcessor.Dto.Import
{
    [XmlType("AnimalAid")]
    public class ImportAnimalAidXmlDto
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string AnimalAidName { get; set; }
    }
}
