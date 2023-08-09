using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;
using UI.ViewModels.EmailViewModels;

namespace UI.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public UserController(IEmailService emailService, IUserService userService, UserManager<AppUser> userManager)
    {
        _emailService = emailService;
        _userService = userService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userViewModel = await _userService.GetInfoUserByCurrentUserAsync(User);

        return View(userViewModel.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        var userViewModel = await _userService.GetInfoUserByIdAsync(id);

        return View(userViewModel.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }
        
        var editUserViewModel = new EditUserViewModel();
        user.MapTo(editUserViewModel);

        return View(editUserViewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Failed to edit profile");
            return View("Edit", editUserViewModel);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var userViewModel = new AppUser();
        editUserViewModel.MapTo(userViewModel);

        var result = await _userService.EditUserAsync(user, userViewModel);

        if (!result.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "edit is not successful. Please try again!");
            return View("Edit", editUserViewModel);
        }

        return RedirectToAction("Index", "User", new { userViewModel.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditPassword()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var editUserPasswordViewModel = new EditUserPasswordViewModel();

        return View(editUserPasswordViewModel);
    }

    [HttpPost]
    [ActionName("EditPassword")]
    public async Task<IActionResult> EditUserPassword(EditUserPasswordViewModel editUserPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Failed to edit password");
            return View("EditPassword", editUserPasswordViewModel);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, editUserPasswordViewModel.CurrentPassword);

        if (checkPassword)
        {
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, editUserPasswordViewModel.NewPassword);
            await _userManager.UpdateAsync(user);

            TempData.TempDataMessage("SuccessMessage", "password has been successfully changed.");

            return RedirectToAction("Index", "User", new { user.Id });
        }
        else
        {
            TempData.TempDataMessage("Error", "You entered incorrect password");
            return View("EditPassword", editUserPasswordViewModel);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        //Comes to the e-mail Admin, he or she have to confirm the deletion of the account
        var callbackUrl = Url.Action(
            "ConfirmUserDeletion",
            "User",
            new { userId = user.Id },
            protocol: HttpContext.Request.Scheme);

        var deletionSendingResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByAdmin, user, callbackUrl);

        if (!deletionSendingResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deletionSendingResult.Message);
        }

        TempData.TempDataMessage("Error", "Wait for confirmation of account deletion from the admin");
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmUserDeletion(string userId)
    {
        if (userId == null)
        {
            return View("Error");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return View("Error");
        }

        //send message to user to delete his or her account
        var actionLink = Url.Action(
            "Delete",
            "Account",
            new { userId = userId },
            protocol: HttpContext.Request.Scheme);

        await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByUser, user, actionLink);
        
        var confirmDeleteVM = new ConfirmUserDeleteViewModel();
        user.MapTo(confirmDeleteVM);

        return View(confirmDeleteVM); //You succesfully confirmed user deletion
    }
}