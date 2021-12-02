using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Instagraph.DataProcessor.Dto.Import
{
    [XmlType("comment")]
    public class ImportCommentDto
    {

        [Required]
        [MaxLength(250)]
        [XmlElement("content")]
        public string Content { get; set; }

        [Required]
        [XmlElement("user")]
        public string Username { get; set; }

        [Required]
        [XmlElement("post")]
        public ImportCommentPostDto Post { get; set; }
    }
}
