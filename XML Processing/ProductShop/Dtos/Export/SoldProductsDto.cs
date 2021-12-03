using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("SoldProducts")]
    public class SoldProductsDto
    {

        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public exp[] Products { get; set; }
    }
}
