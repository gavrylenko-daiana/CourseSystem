using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels.GeneralViewModels;

namespace UI.Controllers;

[Authorize]
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