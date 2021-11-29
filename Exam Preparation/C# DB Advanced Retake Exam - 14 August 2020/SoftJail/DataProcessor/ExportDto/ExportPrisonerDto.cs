using System.Linq;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Prisoner")]
    public class ExportPrisonerDto
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("CellNumber")]
        public int CellNumber { get; set; }

        [XmlArray("Officers")]
        public ExportOfficerDto[] Officers { get; set; }

        [XmlElement("TotalOfficerSalary")]
        public decimal TotalOfficerSalary { get; set; }
    }
}
