using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Cinema.DataProcessor.ImportDto
{
    [XmlType("Customer")]
    public class ImportCustomerDto
    {
        [XmlElement("FirstName")]
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [XmlElement("LastName")]
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string LastName { get; set; }

        [XmlElement("Age")]
        [Range(12, 110)]
        public int Age { get; set; }

        [XmlElement("Balance")]
        [Range(0.01, Double.MaxValue)]
        public decimal Balance { get; set; }

        [XmlArray("Tickets")]
        public ImportTicketDto[] Tickets { get; set; }
    }
}

