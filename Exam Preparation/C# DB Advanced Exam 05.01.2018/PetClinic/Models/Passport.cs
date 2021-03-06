using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetClinic.Models
{
    public class Passport
    {
        [Key]
        [Required]
        public string SerialNumber { get; set; }

        [Required]
        public virtual Animal Animal { get; set; }

        [Required]
        public string OwnerPhoneNumber { get; set; }

        [Required]
        [MaxLength(30)]
        public string OwnerName { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}
