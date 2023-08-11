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

    public UserActivityController(UserManager<AppUser> userManager, IActivityService activityService)
    {
        _userManager = userManager;
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForMonth(DateTime? dateTime = null)
    {
        var month = dateTime ?? DateTime.Now;

        var currentUser = await _userManager.GetUserAsync(User);

        var activities = currentUser.UserActivities.ForMonth(month) ?? throw new ArgumentNullException("currentUser.UserActivities.ForMonth(month)");

        ViewData["DateTime"] = month;

        return View(activities);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityForDay(DateTime? thisDay)
    {
        var day = thisDay ?? DateTime.Today;
        var currentUser = await _userManager.GetUserAsync(User);

        var activities = currentUser.UserActivities.ForDate(day) ?? throw new ArgumentNullException("currentUser.UserActivities.ForDate(day)");

        ViewData["DateTime"] = day;

        return View(activities);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityDetails(int id)
    {
        var activityResult = await _activityService.GetById(id);

        if (!activityResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{activityResult.Message}");
            // edit path
            return RedirectToAction("Index", "Home");
        }

        return View(activityResult.Data);
    }
}