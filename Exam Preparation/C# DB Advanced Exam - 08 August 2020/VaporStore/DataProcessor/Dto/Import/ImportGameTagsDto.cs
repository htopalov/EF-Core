using System.ComponentModel.DataAnnotations;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportGameTagsDto
    {
        [Required]
        public string Name { get; set; }
    }
}
