using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locationvoitures.Models
{
    // L'énumérateur StatutReservation est défini ici, une seule fois.
    public enum StatutReservation
    {
        EnAttente,
        Confirmée,
        Annulée,
        Terminée
    }

    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string ClientId { get; set; }

        [Required]
        public int VoitureId { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public DateTime DateFin { get; set; }

        [Required]
        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;

        [Required]
        public DateTime DateModification { get; set; } = DateTime.Now;
        //annotation
        [Required]// il ne peut pas etre null
        // définir un type  a un attribut 
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [NotMapped]
        public int DureeJours => (DateFin - DateDebut).Days;

        // Relations de navigation
        public virtual ApplicationUser? Client { get; set; }
        public virtual Voiture? Voiture { get; set; }
        public virtual Paiement? Paiement { get; set; }
    }
}