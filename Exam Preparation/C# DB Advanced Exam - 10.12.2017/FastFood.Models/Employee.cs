using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.Models
{
	public class Employee
	{
        public Employee()
        {
            this.Orders = new HashSet<Order>();
        }

		[Key]
        public int Id { get; set; }

		[Required]
		[MaxLength(30)]
        public string Name { get; set; }

        public int Age { get; set; }

		[ForeignKey(nameof(Position))]
        public int PositionId { get; set; }
        public virtual Position Position { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
	}
}