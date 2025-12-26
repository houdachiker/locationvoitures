using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using locationvoitures.Models;

namespace locationvoitures.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Vos entités métier
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Contact> Contacts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚠ Toujours en premier

            // Relation : Reservation → ApplicationUser (client)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId) // Doit être string
                .OnDelete(DeleteBehavior.Restrict);

            // Relation : Voiture → réservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Voiture)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VoitureId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation 1:1 : Paiement ↔ Reservation
            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Paiement)
                .HasForeignKey<Paiement>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}