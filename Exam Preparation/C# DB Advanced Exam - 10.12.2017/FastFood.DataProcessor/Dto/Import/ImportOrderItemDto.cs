using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace FastFood.DataProcessor.Dto.Import
{
    [XmlType("Item")]
    public class ImportOrderItemDto
    {
        [XmlElement("Name")]
        [Required]
        public string Name { get; set; }

        [XmlElement("Quantity")]
        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }
    }
}
