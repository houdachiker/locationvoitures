using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using locationvoitures.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

// Correction : Utilisation du Constructeur Principal (C# 12 / .NET 8)
namespace locationvoitures.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // ==========================================================
        // ➤ 1. Tableau de bord (Dashboard) - CORRIGÉ POUR LA STABILITÉ
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            // 🔑 CORRECTION CRITIQUE : Évaluation côté client pour garantir la stabilité et éviter InvalidOperationException.
            var toutesLesVoitures = await _context.Voitures.ToListAsync() ?? new List<Voiture>();

            int totalVoitures = toutesLesVoitures.Count;

            // Décomptes effectués en mémoire C# (plus sûr que LINQ pour les strings)
            int disponibles = toutesLesVoitures.Count(v => string.Equals(v.Statut, "Disponible", StringComparison.OrdinalIgnoreCase));
            int maintenance = toutesLesVoitures.Count(v => string.Equals(v.Statut, "En maintenance", StringComparison.OrdinalIgnoreCase));
            int reservees = toutesLesVoitures.Count(v => string.Equals(v.Statut, "Réservée", StringComparison.OrdinalIgnoreCase));

            ViewBag.TotalVoitures = totalVoitures;
            ViewBag.Disponibles = disponibles;
            ViewBag.Maintenance = maintenance;
            ViewBag.Reservees = reservees;

            // --- NOUVEAU CODE POUR LES PROCHAINES RÉSERVATIONS ---

            // Définir la plage de temps (Aujourd'hui jusqu'à 7 jours)
            DateTime debutPeriode = DateTime.Today;
            DateTime finPeriode = DateTime.Today.AddDays(7);

            // Récupérer les réservations qui débutent dans cette période
            var prochainesReservations = await _context.Reservations
                // La date de début est dans la fenêtre [Aujourd'hui, Aujourd'hui + 7 jours]
                .Where(r => r.DateDebut.Date >= debutPeriode && r.DateDebut.Date <= finPeriode)
                // Exclure les réservations déjà terminées ou annulées (si votre enum le permet)
                .Where(r => r.Statut != StatutReservation.Terminée && r.Statut != StatutReservation.Annulée)
                .Include(r => r.Client)
                .Include(r => r.Voiture)
                .OrderBy(r => r.DateDebut)
                .Take(5) // Limiter l'affichage à 5 pour le tableau de bord
                .ToListAsync();

            ViewBag.ProchainesReservations = prochainesReservations;

            // --------------------------------------------------------
            // NOTE : TotalClients n'est plus calculé dans le code fourni, s'il est nécessaire dans la vue, utilisez: 
            // ViewBag.TotalClients = await _context.Users.CountAsync();

            return View();
        }

        // ==========================================================
        // ➤ 2. Gestion des Réservations (CORRIGÉ DES AVERTISSEMENTS)
        // ==========================================================

        public async Task<IActionResult> GestionReservations()
        {
            var reservations = await _context.Reservations
                                            .Include(r => r.Voiture)
                                            .Include(r => r.Client)
                                            .OrderByDescending(r => r.DateDebut)
                                            .ToListAsync();

            return View("~/Views/GestionReservations/Index.cshtml", reservations ?? new List<Reservation>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminerReservation(int reservationId)
        {
            var reservation = await _context.Reservations
                                            .Include(r => r.Voiture)
                                            .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                TempData["Erreur"] = "Réservation non trouvée.";
                return RedirectToAction(nameof(GestionReservations));
            }

            // Correction : Sûr car on a vérifié que le Voiture est inclus
            var voiture = reservation.Voiture ?? throw new InvalidOperationException("Voiture non trouvée dans la réservation.");

            // Correction : Utilisation de string.Equals pour la comparaison en mémoire (hors LINQ)
            bool isReservedOrPending = string.Equals(voiture.Statut, "Réservée", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(voiture.Statut, "En attente", StringComparison.OrdinalIgnoreCase);

            if (isReservedOrPending)
            {
                voiture.Statut = "Disponible";
            }

            reservation.Statut = StatutReservation.Terminée;
            reservation.DateModification = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Réservation n°{reservation.Id} terminée. La voiture {voiture.Marque} est de nouveau disponible.";
            return RedirectToAction(nameof(GestionReservations));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations
                .Include(r => r.Voiture)
                .Include(r => r.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null) return NotFound();
            return View("~/Views/GestionReservations/Details.cshtml", reservation);
        }

        public async Task<IActionResult> EditStatut(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            ViewBag.Statuts = System.Enum.GetValues<StatutReservation>()
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() })
                .ToList();
            return View("~/Views/GestionReservations/EditStatut.cshtml", reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatut(int id, string Statut)
        {
            var reservation = await _context.Reservations.Include(r => r.Voiture).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            if (System.Enum.TryParse<StatutReservation>(Statut, true, out var statutEnum))
            {
                reservation.Statut = statutEnum;

                // Correction : Sûr car on a vérifié que le Voiture est inclus
                var voiture = reservation.Voiture ?? throw new InvalidOperationException("Voiture non trouvée dans la réservation.");

                if (statutEnum == StatutReservation.Annulée || statutEnum == StatutReservation.Terminée)
                {
                    voiture.Statut = "Disponible";
                }
                else if (statutEnum == StatutReservation.Confirmée)
                {
                    voiture.Statut = "Réservée";
                }

                reservation.DateModification = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Statut de la réservation mis à jour !";
            }
            else
            {
                TempData["Erreur"] = "Statut invalide.";
            }
            return RedirectToAction(nameof(GestionReservations));
        }

        // ==========================================================
        // ➤ 3. CRUD POUR VOITURES (Nettoyé des avertissements)
        // ==========================================================

        public async Task<IActionResult> Gestionvoitures()
        {
            var voitures = await _context.Voitures.ToListAsync() ?? new List<Voiture>();
            return View(voitures);
        }

        public IActionResult AjouterVehicule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterVehicule(Voiture voiture)
        {
            if (ModelState.IsValid)
            {
                voiture.Statut ??= "Disponible";
                _context.Voitures.Add(voiture);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Voiture ajoutée avec succès !";
                return RedirectToAction(nameof(Gestionvoitures));
            }
            return View(voiture);
        }

        public async Task<IActionResult> ModifierVoiture(int? id)
        {
            if (id == null) return NotFound();
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null) return NotFound();
            return View(voiture);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierVoiture(int id, Voiture voitureEdit)
        {
            if (id != voitureEdit.Id) return NotFound();
            if (!ModelState.IsValid) return View(voitureEdit);

            var voitureExistante = await _context.Voitures.FindAsync(id);
            if (voitureExistante == null) return NotFound();

            try
            {
                voitureExistante.Marque = voitureEdit.Marque;
                voitureExistante.Modele = voitureEdit.Modele;
                voitureExistante.Annee = voitureEdit.Annee;
                voitureExistante.Kilometrage = voitureEdit.Kilometrage;
                voitureExistante.PrixJour = voitureEdit.PrixJour;
                voitureExistante.Statut = voitureEdit.Statut;

                if (!string.IsNullOrWhiteSpace(voitureEdit.Photo))
                {
                    voitureExistante.Photo = voitureEdit.Photo;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Voiture modifiée avec succès !";
                return RedirectToAction(nameof(Gestionvoitures));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoitureExists(voitureExistante.Id)) return NotFound();
                else throw;
            }
        }

        public async Task<IActionResult> SupprimerVoiture(int? id)
        {
            if (id == null) return NotFound();
            var voiture = await _context.Voitures.FirstOrDefaultAsync(m => m.Id == id);
            if (voiture == null) return NotFound();
            return View("~/Views/Admin/SupprimerVoiture.cshtml", voiture);
        }

        [HttpPost, ActionName("SupprimerVoiture")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerVehiculeConfirmed(int id)
        {
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture != null)
            {
                _context.Voitures.Remove(voiture);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Voiture supprimée !";
            }
            return RedirectToAction(nameof(Gestionvoitures));
        }

        public async Task<IActionResult> DetailsVoitures(int? id)
        {
            if (id == null) return NotFound();
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null) return NotFound();
            return View(voiture);
        }

        private bool VoitureExists(int id)
        {
            return _context.Voitures.Any(e => e.Id == id);
        }
    }

}