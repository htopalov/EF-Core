using System.Xml.Serialization;

namespace PetClinic.DataProcessor.Dto.Export
{
    [XmlType("Procedure")]
    public class ExportProcedureDtoXml
    {
        [XmlElement("Passport")]
        public string PassportNumber { get; set; }

        [XmlElement("OwnerNumber")]
        public string OwnerPhoneNumber { get; set; }

        [XmlElement("DateTime")]
        public string DateTime { get; set; }

        [XmlArray("AnimalAids")]
        public ExportAnimalAidsDtoXml[] AnimalAids { get; set; }

        [XmlElement("TotalPrice")]
        public decimal TotalPrice { get; set; }
    }
}



//< Procedure >
//    < Passport > acattee321 </ Passport >
//    < OwnerNumber > 0887446123 </ OwnerNumber >
//    < DateTime > 14 - 01 - 2016 </ DateTime >
//    < AnimalAids >
//    < AnimalAid >
//    < Name > Internal Deworming </ Name >
   
//    < Price > 8.00 </ Price >
   
//    </ AnimalAid >
   
//    < AnimalAid >
   
//    < Name > Fecal Test </ Name >
      
//    < Price > 7.50 </ Price >
      
//    </ AnimalAid >
      
//    < AnimalAid >
      
//    < Name > Nasal Bordetella </ Name >
         
//    < Price > 5.60 </ Price >
         
//    </ AnimalAid >
         
//    </ AnimalAids >
         
//    < TotalPrice > 21.10 </ TotalPrice >
         
//    </ Procedure >
