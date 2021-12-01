using System;
using System.ComponentModel.DataAnnotations;

namespace PetClinic.DataProcessor.Dto.Import
{
    public class ImportAnimalAidDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Name { get; set; }

        [Range(0.01, Double.MaxValue)]
        public decimal Price { get; set; }
    }
}
