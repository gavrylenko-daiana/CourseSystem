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
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(IUserService userService, INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _userService = userService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ViewAll()
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var notificationsResult = currentUserResult.Data.Notifications.OrderByDate();

        if (!notificationsResult.IsSuccessful)
        {
            _logger.LogError("Notifications fail for user {userId}! Error: {errorMessage}", 
                currentUserResult.Data.Id, notificationsResult.Message);
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
            _logger.LogError("Failed to get notification by Id {notificationId}! Error: {errorMessage}", 
                id, notificationResult.Message);
            TempData.TempDataMessage("Error", notificationResult.Message);

            return RedirectToAction("ViewAll");
        }

        await _notificationService.MarkAsRead(notificationResult.Data);

        return View(notificationResult.Data);
    }
}