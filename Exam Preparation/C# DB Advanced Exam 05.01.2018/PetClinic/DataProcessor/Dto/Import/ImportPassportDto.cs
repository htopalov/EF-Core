using System.ComponentModel.DataAnnotations;

namespace PetClinic.DataProcessor.Dto.Import
{
    public class ImportPassportDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]{7}[0-9]{3}$")]
        public string SerialNumber { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string OwnerName { get; set; }

        [Required]
        [RegularExpression(@"(^[0-9]{10})|(\+359[0-9]{9})$")]
        public string OwnerPhoneNumber { get; set; }

        [Required]
        public string RegistrationDate { get; set; }
    }
}
