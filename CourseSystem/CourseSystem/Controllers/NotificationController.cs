using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Core.Enums;
using X.PagedList;
using static Dropbox.Api.Files.ListRevisionsMode;

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
    public async Task<IActionResult> ViewAll(int? page, FilterParam findForFilter, int? findForId, SortingParam sortOrder)
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        if (findForFilter != FilterParam.All)
        {
            ViewBag.FindForParam = findForFilter;
        }

        if (findForId != null)
        {
            ViewBag.FindForId = findForId;
        }

        ViewBag.SortOrder = sortOrder == SortingParam.Created ? SortingParam.Created : SortingParam.CreatedDesc;

        var notificationsResult = await _notificationService.GetNotifications(currentUserResult.Data,
            findForFilter, findForId, sortOrder);

        if (!notificationsResult.IsSuccessful)
        {
            _logger.LogError("Failed to get notifications for user {userId}! Error: {errorMessage}",
                currentUserResult.Data.Id, notificationsResult.Message);

            TempData.TempDataMessage("Error", notificationsResult.Message);

            return RedirectToAction("Index", "Home");
        }

        int pageSize = 10;
        int pageNumber = (page ?? 1);

        return View(notificationsResult.Data.ToPagedList(pageNumber, pageSize));
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

    [HttpGet]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var notifications = currentUserResult.Data.Notifications.Where(n => !n.IsRead);

        foreach (var notification in notifications)
        {
            await _notificationService.MarkAsRead(notification);
        }

        return PartialView("ViewNew");
    }
}