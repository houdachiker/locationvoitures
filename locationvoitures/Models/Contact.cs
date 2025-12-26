
using System.ComponentModel.DataAnnotations;

namespace locationvoitures.Models
{
    public class Contact
    {   
        public int Id { get; set; }
    
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Veuillez entrer une adresse e-mail valide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'objet est obligatoire.")]
        public string Sujet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le message est obligatoire.")]
        public string Message { get; set; } = string.Empty;
        public bool EstLu { get; set; } = false;

    }
}