using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using locationvoitures.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace locationvoitures.Controllers
{
    // Utilisation du Constructeur Principal (syntaxe moderne)
    public class ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // =========================================================================
        // ➤ ACTION 1 : HISTORIQUE DE RÉSERVATIONS (Client Authentifié)
        // =========================================================================
        [Authorize]
        public async Task<IActionResult> Historique()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var historiqueReservations = await _context.Reservations
                .Where(r => r.ClientId == userId)
                .Include(r => r.Voiture)
                .OrderByDescending(r => r.DateDebut)
                .ToListAsync();

            return View(historiqueReservations ?? Enumerable.Empty<Reservation>());
        }

        // =========================================================================
        // ➤ ACTION 2 : Index (Recherche de voitures)
        // =========================================================================
        public async Task<IActionResult> Index(RechercheVoitureViewModel model)
        {
            // CORRECTION: Utilisation de ToLower() pour une traduction LINQ-SQL réussie
            var query = _context.Voitures
                .Where(v => v.Statut != null &&
                            v.Statut.Trim().ToLower() == "disponible");

            if (!string.IsNullOrEmpty(model.Marque))
            {
                query = query.Where(v => v.Marque != null &&
                                         v.Marque.ToLower().Contains(model.Marque.ToLower()));
            }
            if (!string.IsNullOrEmpty(model.Modele))
            {
                query = query.Where(v => v.Modele != null &&
                                         v.Modele.ToLower().Contains(model.Modele.ToLower()));
            }
            if (model.PrixMax.HasValue)
            {
                query = query.Where(v => v.PrixJour <= model.PrixMax.Value);
            }

            var voitures = await query.ToListAsync();
            ViewBag.Voitures = voitures;
            ViewBag.SearchModel = model;
            return View();
        }

        // =========================================================================
        // ➤ ACTION 3 : Create (GET)
        // =========================================================================
        [Authorize]
        public async Task<IActionResult> Create(int voitureId)
        {
            // CORRECTION: Utilisation de ToLower() pour une traduction LINQ-SQL réussie
            var voiture = await _context.Voitures
                .FirstOrDefaultAsync(v => v.Id == voitureId &&
                                         v.Statut != null &&
                                         v.Statut.Trim().ToLower() == "disponible");
            if (voiture == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();

            ViewBag.Voiture = voiture;
            ViewBag.Prenom = user.Prenom;
            ViewBag.Telephone = user.PhoneNumber;

            return View();
        }

        // =========================================================================
        // ➤ ACTION 4 : Create (POST)
        // =========================================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int voitureId,
            DateTime dateDebut,
            DateTime dateFin,
            string? remarques,
            [Required(ErrorMessage = "Le prénom est obligatoire.")] string prenom,
            [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")] string telephone)
        {
            // ... (validation omise pour la brièveté) ...
            if (string.IsNullOrWhiteSpace(prenom))
            {
                TempData["Erreur"] = "Le prénom est obligatoire.";
                return RedirectToAction("Create", new { voitureId });
            }
            // ... (Autres validations) ...

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();

            // CORRECTION: Utilisation de ToLower() pour une traduction LINQ-SQL réussie
            var voiture = await _context.Voitures
                .FirstOrDefaultAsync(v => v.Id == voitureId &&
                                         v.Statut != null &&
                                         v.Statut.Trim().ToLower() == "disponible");
            if (voiture == null)
            {
                TempData["Erreur"] = "Cette voiture n'est plus disponible.";
                return RedirectToAction("Index");
            }

            // ... (mise à jour utilisateur et calcul du montant omis pour la brièveté) ...
            prenom = prenom.Trim();
            telephone = telephone.Trim();

            if (!string.Equals(user.Prenom, prenom, StringComparison.OrdinalIgnoreCase)) { user.Prenom = prenom; }
            if (!string.Equals(user.PhoneNumber, telephone, StringComparison.OrdinalIgnoreCase)) { user.PhoneNumber = telephone; }

            int nbJours = (dateFin.Date - dateDebut.Date).Days;
            if (nbJours <= 0) nbJours = 1;

            // 🔹 Création de la réservation avec l'enum StatutReservation
            var reservation = new Reservation
            {
                ClientId = user.Id!,
                VoitureId = voitureId,
                DateDebut = dateDebut,
                DateFin = dateFin,
                MontantTotal = nbJours * voiture.PrixJour,
                Statut = StatutReservation.Confirmée,
            };

            voiture.Statut = "Réservée";

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Succes"] = "Votre réservation a été confirmée !";
            return RedirectToAction("Historique");
        }

        // =========================================================================
        // ➤ ACTION 5 : Details (Affiche les détails d'une réservation avec photo)
        // =========================================================================
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Récupérer la réservation, inclure la voiture et le paiement
            var reservation = await _context.Reservations 
                .Include(r => r.Voiture)
                .Include(r => r.Client)
                .Include(r => r.Paiement) // Inclure le paiement si vous avez besoin du statut de paiement
                .FirstOrDefaultAsync(m => m.Id == id && m.ClientId == userId);

            if (reservation == null)
            {
                return NotFound(); // Ne doit afficher que les réservations appartenant à l'utilisateur connecté
            }

            return View(reservation);
        }
    }
}