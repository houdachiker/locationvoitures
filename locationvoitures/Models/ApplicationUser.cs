using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace locationvoitures.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        // ✅ Correction : Ajout de 'required'
        public required string Nom { get; set; }

        [Required]
        [StringLength(100)]
        // ✅ Correction : Ajout de 'required'
        public required string Prenom { get; set; }
    }
}