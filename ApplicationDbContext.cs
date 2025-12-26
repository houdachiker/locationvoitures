using Microsoft.EntityFrameworkCore;
using locationVoiture.Models;

namespace locationVoiture.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Un pour chaque modèle
        public DbSet<Client> Clients { get; set; }
        public DbSet<Administrateur> Administrateurs { get; set; }
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Paiement> Paiements { get; set; }

        // Configuration des relations et contraintes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ? Configuration de l'héritage (TPH - Table Per Hierarchy)
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("Type")
                .HasValue<Client>("Client")
                .HasValue<Administrateur>("Administrateur");

            // ? Configuration de la relation Client -> Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

            // ? Configuration de la relation Voiture -> Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Voiture)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VoitureId)
                .OnDelete(DeleteBehavior.Restrict);

            // ? Configuration de la relation Reservation -> Paiement (1:1)
            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Paiement)
                .HasForeignKey<Paiement>(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade); // Si réservation supprimée, paiement aussi

            // ? Configuration de la relation Administrateur -> Voitures
            modelBuilder.Entity<Voiture>()
                .HasOne(v => v.Administrateur)
                .WithMany(a => a.VoituresGerees)
                .HasForeignKey(v => v.AdministrateurId)
                .OnDelete(DeleteBehavior.SetNull); // Si admin supprimé, voiture reste mais sans admin
        }
    }
}