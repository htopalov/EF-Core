using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stations.Models.Enums;

namespace Stations.Models
{
    public class CustomerCard
    {
        public CustomerCard()
        {
            this.BoughtTickets = new HashSet<Ticket>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        public int Age { get; set; }

        public CardType Type { get; set; }

        public virtual ICollection<Ticket> BoughtTickets { get; set; }
    }
}
