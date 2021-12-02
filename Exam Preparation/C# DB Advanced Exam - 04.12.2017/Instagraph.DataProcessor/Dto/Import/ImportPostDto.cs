using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Instagraph.DataProcessor.Dto.Import
{
    [XmlType("post")]
    public class ImportPostDto
    {
        [XmlElement("caption")]
        [Required]
        public string Caption { get; set; }

        [XmlElement("user")]
        [Required]
        [MaxLength(30)]
        public string User { get; set; }

        [XmlElement("picture")]
        [Required]
        public string Picture { get; set; }
    }
}
