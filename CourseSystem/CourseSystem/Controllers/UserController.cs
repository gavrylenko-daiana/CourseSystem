using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;
using UI.ViewModels.EmailViewModels;
using static Dropbox.Api.Files.ListRevisionsMode;

namespace UI.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserController> _logger;

    public UserController(IEmailService emailService, IUserService userService, ILogger<UserController> logger)
    {
        _emailService = emailService;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _userService.GetInfoUserByCurrentUserAsync(User);

        if (result.IsSuccessful)
        {
            return View(result.Data);
        }
        else
        {
            _logger.LogWarning("Unauthorized user");

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        var result = await _userService.GetInfoUserByIdAsync(id);

        if (result.IsSuccessful)
        {
            return View(result.Data);
        }
        else
        {
            _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                id, result.Message);

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var result = await _userService.GetCurrentUser(User);

        if (result.IsSuccessful)
        {
            var editUserViewModel = new EditUserViewModel();
            result.Data.MapTo(editUserViewModel);

            return View(editUserViewModel);
        }
        else
        {
            _logger.LogError("Failed to edit profile - unauthorized user!");

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Failed to edit profile!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Failed to edit profile");

            return View("Edit", editUserViewModel);
        }

        var userViewModel = new AppUser();
        editUserViewModel.MapTo(userViewModel);

        var result = await _userService.EditUserAsync(User, userViewModel, editUserViewModel.NewProfileImage);

        if (!result.IsSuccessful)
        {
            _logger.LogError("Failed to edit user! Error: {errorMessage}", result);

            TempData.TempDataMessage("Error", $"Edit is not successful. Please try again! - Message: {result.Message}");

            return View("Edit", editUserViewModel);
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public async Task<IActionResult> EditPassword()
    {
        var result = await _userService.GetCurrentUser(User);

        if (result.IsSuccessful)
        {
            var editUserPasswordViewModel = new EditUserPasswordViewModel();

            return View(editUserPasswordViewModel);
        }
        else
        {
            _logger.LogError("Failed to edit password - unauthorized user!");

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }

    [HttpPost]
    [ActionName("EditPassword")]
    public async Task<IActionResult> EditUserPassword(EditUserPasswordViewModel editUserPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Failed to edit password!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Failed to edit password");

            return View("EditPassword", editUserPasswordViewModel);
        }

        var resultCheckPassword = await _userService.CheckPasswordAsync(User, editUserPasswordViewModel.CurrentPassword, editUserPasswordViewModel.NewPassword);

        if (resultCheckPassword.IsSuccessful)
        {
            TempData.TempDataMessage("SuccessMessage", "password has been successfully changed.");

            return RedirectToAction("Index", "User");
        }
        else
        {
            _logger.LogWarning("Failed to check password! Error: {errorMessage}", resultCheckPassword.Message);

            TempData.TempDataMessage("Error", "You entered incorrect password");

            return View("EditPassword", editUserPasswordViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditEmail()
    {
        var result = await _userService.GetCurrentUser(User);

        if (result.IsSuccessful)
        {
            return RedirectToAction("SendCodeUser", "Account", new { forgotEntity = ForgotEntity.Email });
        }
        else
        {
            _logger.LogError("Failed to edit password - unauthorized user!");

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete()
    {
        var result = await _userService.GetCurrentUser(User);

        if (result.IsSuccessful)
        {
            var user = result.Data;

            //Comes to the e-mail Admin, he or she have to confirm the deletion of the account
            var callbackUrl = Url.Action(
                "ConfirmUserDeletion",
                "User",
                new { userId = user.Id },
                protocol: HttpContext.Request.Scheme);

            var deletionSendingResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByAdmin, user, callbackUrl);

            if (!deletionSendingResult.IsSuccessful)
            {
                _logger.LogError("Failed to send email with user {userId} account's deletion confirmation request to admins! Error: {errorMessage}",
                    user.Id, deletionSendingResult.Message);

                TempData.TempDataMessage("Error", deletionSendingResult.Message);

                return RedirectToAction("Login", "Account");
            }

            TempData.TempDataMessage("Error", "Wait for confirmation of account deletion from the admin");

            return RedirectToAction("Login", "Account");
        }
        else
        {
            _logger.LogError("Failed to delete password - unauthorized user!");

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmUserDeletion(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("Failed to confirm user deletion - user Id wasn't received!");

            TempData.TempDataMessage("Error", $"Failed to get user by id");

            return RedirectToAction("Index", "User");
        }

        var result = await _userService.FindByIdAsync(userId);

        if (result.IsSuccessful)
        {
            var user = result.Data;

            //send message to user to delete his or her account
            var actionLink = Url.Action(
                "Delete",
                "Account",
                new { userId = userId },
                protocol: HttpContext.Request.Scheme);

            var deletionConfirmedEmailResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByUser, user, actionLink);

            if (!deletionConfirmedEmailResult.IsSuccessful)
            {
                _logger.LogError("Failed to send email with user {userId} account's deletion confirmation to user! Error: {errorMessage}",
                    user.Id, deletionConfirmedEmailResult.Message);

                TempData.TempDataMessage("Error", deletionConfirmedEmailResult.Message);

                return RedirectToAction("ConfirmUserDeletion", "User", userId);
            }

            var confirmDeleteVM = new ConfirmUserDeleteViewModel();
            user.MapTo(confirmDeleteVM);

            return View(confirmDeleteVM); //You succesfully confirmed user deletion
        }
        else
        {
            _logger.LogError("Failed to get user by Id {userId}! Error: {errorMessage}",
                userId, result.Message);

            TempData.TempDataMessage("Error", $"Failed to get {nameof(result.Data)} - Message: {result.Message}");

            return RedirectToAction("Index", "User");
        }
    }
}