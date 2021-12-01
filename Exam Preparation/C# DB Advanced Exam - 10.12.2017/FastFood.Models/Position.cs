using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastFood.Models
{
    public class Position
    {
        public Position()
        {
            this.Employees = new HashSet<Employee>();
        }

        [Key]
        public int Id { get; set; }

        //unique in fluent api-db context
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
