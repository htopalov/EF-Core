using System;
using System.ComponentModel.DataAnnotations;

namespace PetClinic.DataProcessor.Dto.Import
{
    public class ImportAnimalDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Type { get; set; }

        [Range(1,Int32.MaxValue)]
        public int Age { get; set; }

        public ImportPassportDto Passport { get; set; }
    }
}
