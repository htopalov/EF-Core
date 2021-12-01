using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace FastFood.DataProcessor.Dto.Import
{
    [XmlType("Order")]
    public class ImportOrderDto
    {
        [XmlElement("Customer")]
        [Required]
        public string Customer { get; set; }

        [XmlElement("Employee")]
        [Required]
        public string Employee { get; set; }

        [XmlElement("DateTime")]
        [Required]
        public string DateTime { get; set; }

        [XmlElement("Type")]
        [Required]
        public string Type { get; set; }

        [XmlArray("Items")]
        public ImportOrderItemDto[] Items { get; set; }
    }
}

    //< Order >
    //< Customer > Garry </ Customer >
    //< Employee > Maxwell Shanahan </ Employee >
   
    //< DateTime > 21 / 08 / 2017 13:22 </ DateTime >
          
    //< Type > ForHere </ Type >
          
    //< Items >
          
    //< Item >
          
    //< Name > Quarter Pounder </ Name >
             
    //< Quantity > 2 </ Quantity >
             
    //</ Item >
             
    //< Item >
             
    //< Name > Premium chicken sandwich</Name>
    //<Quantity>2</Quantity>
    //</Item>
    //<Item>
    //<Name>Chicken Tenders</Name>
    //<Quantity>4</Quantity>
    //</Item>
    //<Item>
    //<Name>Just Lettuce</Name>
    //<Quantity>4</Quantity>
    //</Item>
    //</Items>
    //</Order>

