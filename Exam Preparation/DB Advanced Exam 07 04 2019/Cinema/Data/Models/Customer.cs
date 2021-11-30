using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Data.Models
{
    public class Customer
    {
        public Customer()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Range(12,110)]
        public int Age { get; set; }

        [Range(0.01, Double.MaxValue)]
        public decimal Balance { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
