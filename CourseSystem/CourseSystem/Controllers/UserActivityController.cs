using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using Core.Helpers;

namespace UI.Controllers
{
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

            var activities = currentUser.UserActivities
                .Where(a => a.Created.Month == month.Month)
                    .OrderByDescending(a => a.Created)
                        .ToList();

            ViewData["DateTime"] = month;

            return View(activities);
        }

        [HttpGet]
        public async Task<IActionResult> ActivityForDay(DateTime? thisDay)
        {
            var day = thisDay ?? DateTime.Today;
            var currentUser = await _userManager.GetUserAsync(User);

            var activities = currentUser.UserActivities
                .Where(a => a.Created.Day == day.Day)
                    .OrderByDescending(a => a.Created)
                        .ToList();

            ViewData["DateTime"] = day;

            return View(activities);
        }

        [HttpGet]
        public async Task<IActionResult> ActivityDetails(int id)
        {
            var activity = await _activityService.GetById(id);

            if (activity == null)
            {
                TempData.TempDataMessage("Error", "This activity does not exist.");

                // edit path
                return RedirectToAction("Index", "Home");
            }

            return View(activity);
        }
    }
}
