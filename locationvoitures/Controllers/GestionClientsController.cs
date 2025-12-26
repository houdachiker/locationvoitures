using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using locationvoitures.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace locationvoitures.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GestionClientsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GestionClientsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 🔹 Liste des clients — Exclut les admins, même s'ils ont le rôle "Client"
        public async Task<IActionResult> Index()
        {
            var clients = new List<ApplicationUser>();

            var allUsers = _userManager.Users.ToList();

            foreach (var user in allUsers)
            {
                bool isClient = await _userManager.IsInRoleAsync(user, "Client");
                bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                // N'inclure QUE les utilisateurs qui sont "Client" ET PAS "Admin"
                if (isClient && !isAdmin)
                {
                    clients.Add(user);
                }
            }

            return View(clients);
        }
        // 🔹 Détails
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "Client"))) return NotFound();

            return View(user);
        }

        // 🔹 Modifier (GET)
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "Client"))) return NotFound();

            return View(user);
        }

        // 🔹 Modifier (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "Client"))) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.Nom = model.Nom;
                    user.Prenom = model.Prenom;
                    user.PhoneNumber = model.PhoneNumber;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Le client a été modifié avec succès.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Une erreur inattendue s'est produite.");
                }
            }

            return View(model);
        }

        // 🔹 Créer (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 Créer (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password)
        {
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    TempData["SuccessMessage"] = "Le client a été créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        // 🔹 Supprimer (GET) — Page de confirmation
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "Client"))) return NotFound();

            return View(user);
        }

        // 🔹 Supprimer (POST) — Action réelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")] // ← IMPORTANT : même URL que le GET
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && await _userManager.IsInRoleAsync(user, "Client"))
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "Client supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}