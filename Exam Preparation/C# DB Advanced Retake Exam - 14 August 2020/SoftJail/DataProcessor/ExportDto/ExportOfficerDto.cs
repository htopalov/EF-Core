using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Officer")]
    public class ExportOfficerDto
    {
        [XmlElement("OfficerName")]
        public string OfficerName { get; set; }

        [XmlElement("Department")]
        public string Department { get; set; }
    }
}
