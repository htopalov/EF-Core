using System;
using System.ComponentModel.DataAnnotations;

namespace Stations.DataProcessor.Dto.Import
{
    public class ImportSeatDto
    {
        [MaxLength(30)]
        public string Name { get; set; }
        [StringLength(2, MinimumLength = 2)]

        public string Abbreviation { get; set; }
        [Range(0, int.MaxValue)]
        [Required]
        public int? Quantity { get; set; }
    }
}
