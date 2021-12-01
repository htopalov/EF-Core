using System.ComponentModel.DataAnnotations.Schema;

namespace PetClinic.Models
{
    public class ProcedureAnimalAid
    {
        [ForeignKey(nameof(Procedure))]
        public int ProcedureId { get; set; }
        public virtual Procedure Procedure { get; set; }

        [ForeignKey(nameof(AnimalAid))]
        public int AnimalAidId { get; set; }
        public virtual AnimalAid AnimalAid { get; set; }
    }
}
