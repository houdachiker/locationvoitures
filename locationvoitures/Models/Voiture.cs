using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locationvoitures.Models
{
    public class Voiture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Marque { get; set; }

        [Required]
        [StringLength(50)]
        public required string Modele { get; set; }

        [Required]
        public int Annee { get; set; }

        [Required]
        public int Kilometrage { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Précision monétaire
        public decimal PrixJour { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "Disponible";

        [StringLength(500)]
        public string? Photo { get; set; }

        public string? AdministrateurId { get; set; }

        // Relations de navigation
        public virtual ApplicationUser? Administrateur { get; set; }

        // Collection initialisée
        public virtual ICollection<Reservation> Reservations { get; set; } = [];
    }
}