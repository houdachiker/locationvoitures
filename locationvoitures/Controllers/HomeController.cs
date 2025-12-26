using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VotreNomDeProjet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();          // Page d'accueil
            // À propos
        public IActionResult Services() => View();      // Services
        public IActionResult Team() => View();          // Équipe
        public IActionResult Testimonial() => View();   // Témoignages
        public IActionResult Contact() => View();       // Contact
        public IActionResult Listedesvehicules() => View();           // Liste des voitures
             // Détail d'une voiture
        [Authorize(Roles = "Client")]
        public IActionResult Reservation() => View();      // Réservation
        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => View();
    }
}