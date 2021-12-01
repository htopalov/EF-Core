using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetClinic.Models
{
    public class AnimalAid
    {
        public AnimalAid()
        {
            this.ProcedureAnimalAids = new HashSet<ProcedureAnimalAid>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public virtual ICollection<ProcedureAnimalAid> ProcedureAnimalAids { get; set; }
    }
}
