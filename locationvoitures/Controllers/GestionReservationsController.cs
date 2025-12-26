using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // ⚠️ requis pour SelectListItem
using Microsoft.EntityFrameworkCore;
using locationvoitures.Models;

namespace locationvoitures.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GestionReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GestionReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➤ Liste toutes les réservations
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Voiture)
                .OrderByDescending(r => r.DateDebut)
                .ToListAsync();

            return View(reservations);
        }

        // ➤ Détails d'une réservation
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Voiture)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // ➤ Formulaire de modification du statut — CORRIGÉ
        public async Task<IActionResult> EditStatut(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            // 🔽 Conversion de l'enum en SelectListItem
            ViewBag.Statuts = Enum.GetValues<StatutReservation>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e switch
                    {
                        StatutReservation.EnAttente => "En attente",
                        StatutReservation.Confirmée => "Confirmée",
                        StatutReservation.Annulée => "Annulée",
                        StatutReservation.Terminée => "Terminée",
                        _ => e.ToString()
                    }
                })
                .ToList();

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatut(int id, string Statut) // ← string, pas enum
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            // Conversion fiable avec accents
            if (Enum.TryParse<StatutReservation>(Statut, true, out var statutEnum))
            {
                reservation.Statut = statutEnum;
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Statut mis à jour.";
            }
            else
            {
                TempData["ErrorMessage"] = "Statut invalide.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}