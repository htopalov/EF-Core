using System.ComponentModel.DataAnnotations;

namespace Theatre.DataProcessor.ImportDto
{
    public class TheatreImportDto
    {
        [StringLength(30, MinimumLength = 4)]
        public string Name { get; set; }

        [Range(1, 10)]
        public sbyte NumberOfHalls { get; set; }

        [StringLength(30, MinimumLength = 4)]
        public string Director { get; set; }

        public TicketImportDto[] Tickets { get; set; }
    }
}
