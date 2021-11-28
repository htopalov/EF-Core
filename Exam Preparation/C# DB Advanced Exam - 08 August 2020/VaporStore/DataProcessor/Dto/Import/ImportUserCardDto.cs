using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportUserCardDto
    {
        [Required]
        [RegularExpression(@"^\d{4} \d{4} \d{4} \d{4}$")]
        public string Number { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}$")]
        [JsonProperty("CVC")]
        public string Cvc { get; set; }

        [Required]
        [Range(0,1)]
        public string Type { get; set; }
    }
}
