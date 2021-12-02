using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stations.Models.Enums;

namespace Stations.DataProcessor.Dto.Import
{
    public class ImportTrainDto
    {
        [Required]
        [MaxLength(10)]
        public string TrainNumber { get; set; }

        public string Type { get; set; } = "HighSpeed";

        public List<ImportSeatDto> Seats { get; set; } = new List<ImportSeatDto>();
    }
}
