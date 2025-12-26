using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using locationvoitures.Models;
using System.Threading.Tasks;

namespace locationvoitures.Controllers
{
    public class VoituresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoituresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ➤ 1. Liste des voitures (disponibles) — ton action existante
        public async Task<IActionResult> Listedesvehicules()
        {
            var voitures = await _context.Voitures
                .Where(v => v.Statut == "Disponible")
                .ToListAsync();
            return View(voitures);
        }

        // ➤ 2. Liste ADMIN de TOUTES les voitures (pour le CRUD dans l'admin)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Voitures.ToListAsync());
        }

        // ➤ 3. Détail (optionnel)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null) return NotFound();

            return View(voiture);
        }

        // ➤ 4. Afficher le formulaire d'ajout
        public IActionResult Create()
        {
            return View();
        }

        // ➤ 5. Enregistrer une nouvelle voiture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voiture voiture)
        {
            if (ModelState.IsValid)
            {
                _context.Voitures.Add(voiture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // ou Listedesvehicules si tu préfères
            }
            return View(voiture);
        }

        // ➤ 6. Afficher le formulaire de modification
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null) return NotFound();

            return View(voiture);
        }

        // ➤ 7. Mettre à jour la voiture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voiture voiture)
        {
            if (id != voiture.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voiture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoitureExists(voiture.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(voiture);
        }

        // ➤ 8. Afficher la page de suppression
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var voiture = await _context.Voitures.FirstOrDefaultAsync(m => m.Id == id);
            if (voiture == null) return NotFound();

            return View(voiture);
        }

        // ➤ 9. Confirmer la suppression
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture != null)
            {
                _context.Voitures.Remove(voiture);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ➤ Fonction utilitaire
        private bool VoitureExists(int id)
        {
            return _context.Voitures.Any(e => e.Id == id);
        }
    }
}