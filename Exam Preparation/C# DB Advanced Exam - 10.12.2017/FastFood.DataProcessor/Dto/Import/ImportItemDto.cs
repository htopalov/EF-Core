using System;
using System.ComponentModel.DataAnnotations;

namespace FastFood.DataProcessor.Dto.Import
{
    public class ImportItemDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Name { get; set; }

        [Range(0.01, Double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Category { get; set; }
    }
}
