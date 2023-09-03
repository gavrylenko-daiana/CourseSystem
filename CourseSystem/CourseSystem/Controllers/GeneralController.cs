using Core.Enums;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels.GeneralViewModels;

namespace UI.Controllers;

public class GeneralController : Controller
{
    [HttpGet]
    public async Task<IActionResult> MessageForNonexistentEntity(EntityType entityType)
    {
        var entityViewModel = new EntityViewModel()
        {
            EntityType = entityType
        };
        
        return View(entityViewModel);
    }
}