using System;
using System.ComponentModel.DataAnnotations;

namespace Instagraph.DataProcessor.Dto.Import
{
    public class ImportPictureDto
    {
        [Required]
        public string Path { get; set; }

        [Range(0.01, Double.MaxValue)]
        public decimal Size { get; set; }
    }
}
