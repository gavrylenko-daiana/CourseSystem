using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace UI.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notificationService;

    public NotificationController(UserManager<AppUser> userManager, INotificationService notificationService)
    {
        _userManager = userManager;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> ViewNew()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var notifications = currentUser.Notifications.NotRead().OrderByDate() ?? throw new ArgumentNullException("currentUser.Notifications.NotRead().OrderByDate()");

        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> ViewAll()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var notifications = currentUser.Notifications.OrderByDate() ?? throw new ArgumentNullException("currentUser.Notifications.OrderByDate()");

        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> NotificationDetails(int id)
    {
        var notificationResult = await _notificationService.GetById(id);

        if (!notificationResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{notificationResult.Message}");
            // edit path
            return RedirectToAction("Index", "Home");
        }
        
        await _notificationService.MarkAsRead(notificationResult.Data);

        return View(notificationResult.Data);
    }
}