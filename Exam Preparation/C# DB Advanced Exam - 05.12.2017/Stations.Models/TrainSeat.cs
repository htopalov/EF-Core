using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stations.Models
{
    public class TrainSeat
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Train))]
        public int TrainId { get; set; }
        public virtual Train Train { get; set; }

        [ForeignKey(nameof(SeatingClass))]
        public int SeatingClassId { get; set; }
        public virtual SeatingClass SeatingClass { get; set; }

        public int Quantity { get; set; }
    }
}
