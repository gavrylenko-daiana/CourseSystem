using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;

namespace UI.Controllers;

[Authorize]
public class UserActivityController : Controller
{
    private readonly IActivityService _activityService;
    private readonly ILogger<UserActivityController> _logger;
    private readonly IUserService _userService;

    public UserActivityController(IActivityService activityService, ILogger<UserActivityController> logger, IUserService userService)
    {
        _activityService = activityService;
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForMonth(DateTime? dateTime = null)
    {
        var month = dateTime ?? DateTime.Now;

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var activitiesResult = currentUserResult.Data.UserActivities.ForMonth(month);

        if (!activitiesResult.IsSuccessful)
        {
            _logger.LogError("Activities fail for user {userId}! Error: {errorMessage}", currentUserResult.Data.Id, activitiesResult.Message);
            TempData.TempDataMessage("Error", activitiesResult.Message);

            return RedirectToAction("Index", "Home");
        }

        ViewData["DateTime"] = month;

        return View(activitiesResult.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForDay(DateTime? thisDay, int? page)
    {
        var day = thisDay ?? DateTime.Today;

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogWarning("Unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var activitiesResult = currentUserResult.Data.UserActivities.ForDate(day);

        if (!activitiesResult.IsSuccessful)
        {
            _logger.LogError("Activities fail for user {userId}! Error: {errorMessage}", currentUserResult.Data.Id, activitiesResult.Message);
            TempData.TempDataMessage("Error", activitiesResult.Message);

            return RedirectToAction("Index", "Home");
        }
        
        int pageSize = 12;
        int pageNumber = (page ?? 1);
        ViewData["DateTime"] = day;
        ViewBag.ThisDay = day;
        
        return View(activitiesResult.Data.ToPagedList(pageNumber, pageSize));
    }

    [HttpGet]
    public async Task<IActionResult> ActivityDetails(int id)
    {
        var activityResult = await _activityService.GetById(id);

        if (!activityResult.IsSuccessful)
        {
            _logger.LogError("Failed to get notification by Id {activityId}! Error: {errorMessage}", 
                id, activityResult.Message);
            TempData.TempDataMessage("Error", $"{activityResult.Message}");

            return RedirectToAction("Index", "Home");
        }

        return View(activityResult.Data);
    }
}