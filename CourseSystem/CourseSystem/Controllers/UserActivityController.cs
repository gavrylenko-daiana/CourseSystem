using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using Core.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace UI.Controllers;

[Authorize]
public class UserActivityController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IActivityService _activityService;
    private readonly ILogger<UserActivityController> _logger;

    public UserActivityController(UserManager<AppUser> userManager, IActivityService activityService,
        ILogger<UserActivityController> logger)
    {
        _userManager = userManager;
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForMonth(DateTime? dateTime = null)
    {
        var month = dateTime ?? DateTime.Now;
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var activitiesResult = currentUser.UserActivities.ForMonth(month);

        if (!activitiesResult.IsSuccessful)
        {
            _logger.LogError("Activities fail for user {userId}! Error: {errorMessage}", currentUser.Id,
                activitiesResult.Message);
            TempData.TempDataMessage("Error", activitiesResult.Message);

            return RedirectToAction("Index", "Home");
        }

        ViewData["DateTime"] = month;

        return View(activitiesResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForDay(DateTime? thisDay)
    {
        var day = thisDay ?? DateTime.Today;
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var activitiesResult = currentUser.UserActivities.ForDate(day);

        if (!activitiesResult.IsSuccessful)
        {
            _logger.LogError("Activities fail for user {userId}! Error: {errorMessage}", currentUser.Id,
                activitiesResult.Message);
            TempData.TempDataMessage("Error", activitiesResult.Message);

            return RedirectToAction("Index", "Home");
        }

        ViewData["DateTime"] = day;

        return View(activitiesResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityDetails(int id)
    {
        var activityResult = await _activityService.GetById(id);

        if (!activityResult.IsSuccessful)
        {
            _logger.LogError("Failed to get notification by Id {activityId}! Error: {errorMessage}", id,
                activityResult.Message);
            TempData.TempDataMessage("Error", $"{activityResult.Message}");

            return RedirectToAction("Index", "Home");
        }

        return View(activityResult.Data);
    }
}