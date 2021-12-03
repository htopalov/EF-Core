﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CarDealer.Dto.Import
{
    [XmlType("Part")]
    public class ImportPartDto
    {
        [XmlElement("name")]
        [Required]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("supplierId")]
        public int SupplierId { get; set; }
    }
}
