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
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(UserManager<AppUser> userManager, INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ViewNew()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }

        var notificationsResult = currentUser.Notifications.NotReadByDate();

        if (!notificationsResult.IsSuccessful)
        {
            _logger.LogError("Notifications fail for user {userId}! Error: {errorMessage}", currentUser.Id, notificationsResult.Message);
            TempData.TempDataMessage("Error", notificationsResult.Message);
            return RedirectToAction("Index", "Home");
        }

        return View(notificationsResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ViewAll()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            _logger.LogWarning("Unouthirized user");
            return RedirectToAction("Login", "Account");
        }

        var notificationsResult = currentUser.Notifications.OrderByDate();

        if (!notificationsResult.IsSuccessful)
        {
            _logger.LogError("Notifications fail for user {userId}! Error: {errorMessage}", currentUser.Id, notificationsResult.Message);
            TempData.TempDataMessage("Error", notificationsResult.Message);
            return RedirectToAction("Index", "Home");
        }

        return View(notificationsResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> NotificationDetails(int id)
    {
        var notificationResult = await _notificationService.GetById(id);

        if (!notificationResult.IsSuccessful)
        {
            _logger.LogError("Failed to get notification by Id {notificationId}! Error: {errorMessage}", id, notificationResult.Message);
            TempData.TempDataMessage("Error", notificationResult.Message);
            return RedirectToAction("ViewAll");
        }
        
        await _notificationService.MarkAsRead(notificationResult.Data);

        return View(notificationResult.Data);
    }
}