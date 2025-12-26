using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using locationvoitures.Models;           // ← Où se trouve votre modèle Contact


namespace RoyalCar.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Votre message a été envoyé avec succès !";
                return RedirectToAction(nameof(Contact));
            }

            return View(contact);
        }
    }
}