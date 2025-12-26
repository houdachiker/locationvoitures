using Microsoft.AspNetCore.Mvc;

using locationvoitures.Models;
using System.Linq;
using System.Collections.Generic;

public class UnreadContactsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public UnreadContactsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        // Les 10 derniers messages selon l'Id
        var messages = _context.Contacts
            .OrderByDescending(c => c.Id)
            .Take(10)
            .ToList();

        // Nombre total
        ViewBag.MessageCount = messages.Count;

        return View(messages);
    }
}
