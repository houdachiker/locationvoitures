using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locationvoitures.Models
{
    public class Paiement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Précision monétaire
        public decimal Montant { get; set; }

        [Required]
        public DateTime DatePaiement { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "En attente";

        [Required]
        [StringLength(50)]
        public string MethodePaiement { get; set; } = string.Empty; // Initialisation pour non-nullabilité

        [StringLength(200)]
        public string? TransactionId { get; set; } // Nullable

        // Relation
        public virtual Reservation? Reservation { get; set; }
    }
}