using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;
using UI.ViewModels.EmailViewModels;
using static Dropbox.Api.Sharing.ListFileMembersIndividualResult;
using Core.ImageStore;
using static Dropbox.Api.TeamLog.AccessMethodLogInfo;
using System.Security.Claims;

namespace UI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ICourseService _courseService;
    private readonly IUserService _userService;
    private readonly IProfileImageService _profileImageService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager, IEmailService emailService, 
        IUserService userService, IProfileImageService profileImageService, ILogger<AccountController> logger, ICourseService courseService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _userService = userService;
        _profileImageService = profileImageService;
        _logger = logger;
        _courseService = courseService;
    }

    private async Task CreateAppUserRoles()
    {
        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Admin.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Admin.ToString()));
        }

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Teacher.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Teacher.ToString()));
        }

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Student.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Student.ToString()));
        }
    }

    private string CreateCallBackUrl(string code, string controllerName, string actionName, object routeValues)
    {
        if (controllerName.IsNullOrEmpty() || actionName.IsNullOrEmpty())
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

        if (routeValues == null)
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

        var callbackUrl = Url.Action(
            actionName,
            controllerName,
            routeValues,
            protocol: HttpContext.Request.Scheme);

        return callbackUrl;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            _logger.LogError("Authorised user tried to log in the system!");

            return RedirectToAction("Logout", "Account");
        }

        var login = new LoginViewModel()
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };

        return View(login);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        loginViewModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to log in user - invalid input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            TempData.TempDataMessage("Error", "Values must not be empty. Please try again.");

            return View(loginViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(loginViewModel.EmailAddress);

        if (!userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to log in user by email {userEmail}! Error: {errorMessage}", loginViewModel.EmailAddress, userResult.Message);

            ViewData.ViewDataMessage("Error", "Entered incorrect email or password. Please try again.");

            return View(loginViewModel);
        }
        var user = userResult.Data;

        var imageUserResult = await _profileImageService.SetDefaultProfileImage(user);

        if (!imageUserResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to set profile image", imageUserResult.Data);
        }

        if (!ValidationHelpers.IsValidEmail(loginViewModel.EmailAddress))
        {
            _logger.LogWarning("Failed to log in user due to invalid email {userEmail}", loginViewModel.EmailAddress);

            ViewData.ViewDataMessage("Error", "Entered incorrect email. Please try again.");

            return View(loginViewModel);
        }

        if (user.Role != AppUserRoles.Admin)
        {
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("User with not verified email {userEmail} tried to log in!", loginViewModel.EmailAddress);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = user.Id, code = code });

                if (await _userManager.IsInRoleAsync(user, AppUserRoles.Admin.ToString()))
                {
                    await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, user, callbackUrl);
                    
                    TempData.TempDataMessage("Error", "Your ADMIN account is not verified, we sent email for confirmation again");
                }
                else
                {
                    await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, user, callbackUrl);
                    
                    TempData.TempDataMessage("Error", "Admin hasn't verified your email yet, we sent email for confirmation again");
                }

                return View(loginViewModel);
            }
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

        if (checkPassword)
        {
            var singInAttempt = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

            if (singInAttempt.Succeeded)
            {
                return RedirectToAction("Index", "User");
            }
        }

        _logger.LogWarning("Failed to log in user {userId} due to invalid password", user.Id);

        TempData.TempDataMessage("Error", "Entered incorrect email or password. Please try again.");

        return View(loginViewModel);
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account");

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return new ChallengeResult(provider, properties);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogError("Error from external provider: {errorMessgae}", remoteError);

            ViewData.ViewDataMessage("Error", "Something went wrong. Please try again.");
            return RedirectToAction("Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            _logger.LogError("Error loading external login information.");

            ViewData.ViewDataMessage("Error", "Something went wrong. Please try again.");
            return RedirectToAction("Login");
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
            info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return RedirectToAction("Index", "User");
        }

        else
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    var registerVM = new RegisterViewModel()
                    {
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };

                    return View("ExternalRegistration", registerVM);
                }

                if (user.Role != AppUserRoles.Admin)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        _logger.LogWarning("User with not verified email {userEmail} tried to log in!", email);

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = user.Id, code = code });

                        if (await _userManager.IsInRoleAsync(user, AppUserRoles.Admin.ToString()))
                        {
                            await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, user, callbackUrl);

                            TempData.TempDataMessage("Error", "Your ADMIN account is not verified, we sent email for confirmation again");
                        }
                        else
                        {
                            await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, user, callbackUrl);

                            TempData.TempDataMessage("Error", "Admin hasn't verified your email yet, we sent email for confirmation again");
                        }

                        return RedirectToAction("Login");
                    }
                }
                
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "User");
            }

            TempData.TempDataMessage("Error", "Something went wrong. Please try again.");
            return RedirectToAction("Login");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExternalRegister(RegisterViewModel registerViewModel)
    {
        var userResult = await _userService.GetUserByEmailAsync(registerViewModel.Email);

        if (userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to register user by email {userEmail}! Error: user with this email already exists: {userId}",
                registerViewModel.Email, userResult.Data.Id);

            TempData.TempDataMessage("Error", "This email is already in use");

            return RedirectToAction("Register");
        }

        var newUser = new AppUser();
        registerViewModel.MapTo(newUser);

        newUser.UserName = registerViewModel.FirstName + registerViewModel.LastName;

        var newUserResponse = await _userManager.CreateAsync(newUser);

        await CreateAppUserRoles();

        var imageUserResult = await _profileImageService.SetDefaultProfileImage(newUser);

        if (!imageUserResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to set profile image", imageUserResult.Data);
        }

        if (newUserResponse.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(newUser, registerViewModel.Role.ToString());

            if (!roleResult.Succeeded)
            {
                TempData.TempDataMessage("Error", "Failed to add new user");

                return RedirectToAction("Register");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = newUser.Id, code = code });

            if (registerViewModel.Role != AppUserRoles.Admin)
            {
                await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, newUser, callbackUrl);

                TempData.TempDataMessage("Error", "Please, wait for registration confirmation from the admin");

                return RedirectToAction("Register");
            }
            else
            {
                var emailSentResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, newUser, callbackUrl);

                if (!emailSentResult.IsSuccessful)
                {
                    TempData.TempDataMessage("Error", emailSentResult.Message);
                }

                TempData.TempDataMessage("Error", "To complete your ADMIN registration, check your email and follow the link provided in the email");

                return RedirectToAction("Register");
            }
        }
        else
        {
            _logger.LogError("Failed to register user!");

            var errorMessages = string.Empty;

            foreach (var error in newUserResponse.Errors)
            {
                errorMessages += $"{error.Description}{Environment.NewLine}";

                _logger.LogWarning("Error: {errorMessage}", error.Description);
            }

            TempData.TempDataMessage("Error", errorMessages);

            return RedirectToAction("Register");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var register = new RegisterViewModel()
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };

        return View(register);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to register user - invalid input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            return View(registerViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(registerViewModel.Email);

        if (userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to register user by email {userEmail}! Error: user with this email already exists: {userId}", 
                registerViewModel.Email, userResult.Data.Id);

            TempData.TempDataMessage("Error", "This email is already in use");

            return View(registerViewModel);
        }

        if (!ValidationHelpers.IsValidEmail(registerViewModel.Email))
        {
            _logger.LogWarning("Failed to register user due to invalid email {userEmail}", registerViewModel.Email);

            ViewData.ViewDataMessage("Error", "Entered incorrect email. Please try again.");

            return View(registerViewModel);
        }

        var newUser = new AppUser();
        registerViewModel.MapTo(newUser);

        newUser.UserName = registerViewModel.FirstName + registerViewModel.LastName;

        var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

        await CreateAppUserRoles();

        var imageUserResult = await _profileImageService.SetDefaultProfileImage(newUser);

        if (!imageUserResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to set profile image", imageUserResult.Data);
        }

        if (newUserResponse.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(newUser, registerViewModel.Role.ToString());

            if (!roleResult.Succeeded)
            {
                TempData.TempDataMessage("Error", "Failed to add new user");

                return View(registerViewModel);
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = newUser.Id, code = code });

            if (registerViewModel.Role != AppUserRoles.Admin)
            {
                await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, newUser, callbackUrl);

                TempData.TempDataMessage("Error", "Please, wait for registration confirmation from the admin");

                return View(registerViewModel);
            }
            else
            {
                var emailSentResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, newUser, callbackUrl);

                if (!emailSentResult.IsSuccessful)
                {
                    TempData.TempDataMessage("Error", emailSentResult.Message);
                }

                TempData.TempDataMessage("Error", "To complete your ADMIN registration, check your email and follow the link provided in the email");

                return View(registerViewModel);
            }
        }
        else
        {
            _logger.LogError("Failed to register user!");

            var errorMessages = string.Empty;

            foreach (var error in newUserResponse.Errors)
            {
                errorMessages += $"{error.Description}{Environment.NewLine}";

                _logger.LogWarning("Error: {errorMessage}", error.Description);
            }

            TempData.TempDataMessage("Error", errorMessages);

            return View(registerViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            _logger.LogWarning("Failed to confirm user email - code or user Id was null");

            return View("Error");
        }

        var userResult = await _userService.FindByIdAsync(userId);

        if (!userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to confirm email of user {userId}! Error: {errorMessage}", 
                userId, userResult.Message);

            return RedirectToAction("Login", "Account");
        }

        var user = userResult.Data;
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var toUserProfileUrl = CreateCallBackUrl(token, "User", "Detail", new { id = user.Id });

            await _emailService.SendEmailToAppUsers(EmailType.UserRegistration, user, toUserProfileUrl);

            var confirmEmailVM = new ConfirmEmailViewModel();
            user.MapTo(confirmEmailVM);

            return View(confirmEmailVM);
        }
        else
        {
            _logger.LogError("Failed to confirm email of user {userId}!", user.Id);

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Error: {errorMessage}", error.Description);
            }

            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string userId)
    {
        var userResult = await _userService.FindByIdAsync(userId);

        if (userResult.IsSuccessful)
        {
            var deleteResult = await _profileImageService.DeleteUserProfileImage(userResult.Data);

            if (!deleteResult.IsSuccessful)
            {
                _logger.LogWarning("Failed to delete user with id {userId}! Error: {errorMessage}", userId, deleteResult.Message);
            }

            await _signInManager.UserManager.DeleteAsync(userResult.Data);
        }
        else
        {
            _logger.LogWarning("Failed to delete user with id {userId}! Error: {errorMessage}", userId, userResult.Message);
        }

        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        var forgotPasswordViewModel = new ForgotViewModel();

        return View(forgotPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotViewModel forgotPasswordBeforeEnteringViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to send email for password change to unouthorised user - invalid email input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(forgotPasswordBeforeEnteringViewModel.Email);

        if (!userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to send email for password change to unouthorised user by email {userEmail}! Error: {errorMessage}",
                forgotPasswordBeforeEnteringViewModel.Email, userResult.Message);

            TempData.TempDataMessage("Error", "You wrote an incorrect email. Try again!");

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        // Email
        var emailCodeResult = await _emailService.SendCodeToUser(forgotPasswordBeforeEnteringViewModel.Email);

        if (!emailCodeResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to send email for password change to unouthorised user by email {userEmail}! Error: {errorMessage}",
                forgotPasswordBeforeEnteringViewModel.Email, emailCodeResult.Message);

            TempData.TempDataMessage("Error", emailCodeResult.Message);

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        return RedirectToAction("CheckEmailCode", new { code = emailCodeResult.Data, email = forgotPasswordBeforeEnteringViewModel.Email });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SendCodeUser(ForgotEntity forgotEntity)
    {
        var userResult = await _userService.GetCurrentUser(User);

        if (!userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to send code to user - unauthorized user");

            return RedirectToAction("Login", "Account");
        }

        var emailCodeResult = await _emailService.SendCodeToUser(userResult.Data.Email!);

        if (!emailCodeResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to send code to user {userId}! Error: {errorMessage}",
                userResult.Data.Id, emailCodeResult.Message);

            TempData.TempDataMessage("Error", emailCodeResult.Message);

            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("CheckEmailCode", new { code = emailCodeResult.Data, email = userResult.Data.Email, forgotEntity });
    }

    [HttpGet]
    public IActionResult CheckEmailCode(int code, string email, ForgotEntity forgotEntity)
    {
        var forgotViewModel = new ForgotViewModel()
        {
            Email = email,
            ForgotEntity = forgotEntity
        };

        TempData["Code"] = code;

        return View(forgotViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CheckEmailCode(int code, ForgotViewModel forgotViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to check email code - invalid code input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            return View(forgotViewModel);
        }

        TempData["ForgotEntity"] = forgotViewModel.ForgotEntity;

        if (code == forgotViewModel.EmailCode)
        {
            if (forgotViewModel.ForgotEntity == ForgotEntity.Password)
            {
                return RedirectToAction("ResetPassword", new { email = forgotViewModel.Email });
            }

            return RedirectToAction("ResetEmail", new { email = forgotViewModel.Email });
        }
        else
        {
            TempData.TempDataMessage("Error", "Invalid code. Please try again.");

            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        TempData.TempDataMessage("SuccessMessage", "Code is valid. You can reset your password.");

        var resetPassword = new NewPasswordViewModel()
        {
            Email = email
        };

        return View(resetPassword);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(NewPasswordViewModel newPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to reset password - invalid input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            return View(newPasswordViewModel);
        }

        var result = await _userService.UpdatePasswordAsync(newPasswordViewModel.Email, newPasswordViewModel.NewPassword);

        if (!result.IsSuccessful)
        {
            _logger.LogError("Failed to reset password for user with email {userEmail}! Error: {errorMessage}",
                newPasswordViewModel.Email, result.Message);

            TempData.TempDataMessage("Error", "Failed to reset password.");
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult RegisterNewAdmin()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterNewAdmin(RegisterAdminViewModel registerAdminViewModel)
    {
        var userResult = await _userService.GetUserByEmailAsync(registerAdminViewModel.Email);

        if (userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to register admin by email {userEmail}! Error: user with this email already exists: {userId}",
                registerAdminViewModel.Email, userResult.Data.Id);

            TempData.TempDataMessage("Error", $"This email already exist");

            return View(registerAdminViewModel);
        }

        var newAdmin = new AppUser();
        registerAdminViewModel.MapTo(newAdmin);
        
        newAdmin.EmailConfirmed = true;
        newAdmin.UserName = newAdmin.FirstName + newAdmin.LastName;

        var newUserResponse = await _userManager.CreateAsync(newAdmin, _userService.GenerateTemporaryPassword());

        if (!newUserResponse.Succeeded)
        {
            _logger.LogError("Failed to register new admin!");

            foreach (var error in newUserResponse.Errors)
            {
                _logger.LogWarning("Error: {errorMessage}", error.Description);
            }

            TempData.TempDataMessage("Error", "Failed to add new admin");

            return View(registerAdminViewModel);
        }
        
        var roleResult = await _userManager.AddToRoleAsync(newAdmin, registerAdminViewModel.Role.ToString());

        if (!roleResult.Succeeded)
        {
            TempData.TempDataMessage("Error", "Failed to add new admin");

            return View(registerAdminViewModel);
        }

        var addAdminToCoursesResult = await _courseService.AddNewAdminToCourses(newAdmin);

        if (!addAdminToCoursesResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "Failed to add new admin to courses");

            return View(registerAdminViewModel);
        }

        var sendResult = await _emailService.SendTempPasswordToUser(EmailType.GetTempPassword, newAdmin);

        if (!sendResult.IsSuccessful)
        {
            _logger.LogError("Failed to send email with temporarupassword to new admin {userId}! Error: {errorMessage}",
                newAdmin.Id, sendResult.Message);

            await _userManager.DeleteAsync(newAdmin);
            
            TempData.TempDataMessage("Error", $"{sendResult.Message}");

            return View(registerAdminViewModel);
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult ResetEmail(string email)
    {
        TempData.TempDataMessage("SuccessMessage", "Code is valid. You can reset your email.");

        var resetEmail = new NewEmailViewModel()
        {
            Email = email
        };

        return View(resetEmail);
    }

    [HttpPost]
    public async Task<IActionResult> ResetEmail(NewEmailViewModel newEmailViewModel)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Failed to reset email - invalid input!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Error: {errorMessage}", error.ErrorMessage);
            }

            return View(newEmailViewModel);
        }

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            _logger.LogError("Failed to reset email - unauthorised user!");

            return RedirectToAction("Login");
        }
        
        var userResult = await _userService.GetUserByEmailAsync(newEmailViewModel.NewEmail);
        
        if (userResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to reset email for user {userId}! Error: user with email {desirableEmail} already exists: {userWithDesirableEmailId}",
                currentUserResult.Data.Id, newEmailViewModel.NewEmail, userResult.Data.Id);

            TempData.TempDataMessage("Error", $"This email already exist");

            return View(newEmailViewModel);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(currentUserResult.Data);

        var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmedResetEmail", 
            new
                {
                    userId = currentUserResult.Data.Id, code = code, currentEmail = newEmailViewModel.Email,
                    newEmail = newEmailViewModel.NewEmail
                });

        var oldEmail = currentUserResult.Data.Email;

        currentUserResult.Data.Email = newEmailViewModel.NewEmail;

        var emailResult = await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByUser, currentUserResult.Data, callbackUrl);

        if (!emailResult.IsSuccessful)
        {
            _logger.LogWarning("Failed to send letter with email change confirmation to email {userEmail} for user {userId}! Error: {errorMessage}",
                newEmailViewModel.NewEmail, userResult.Data.Id, emailResult.Data);

            currentUserResult.Data.Email = oldEmail;

            TempData.TempDataMessage("Error", "Something went wrong! Your email wasn't changed. Please, try again.");

            return RedirectToAction("Index", "Home");
        }

        TempData.TempDataMessage("Error", "Please, wait for registration confirmation from the admin");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmedResetEmail(string userId, string code, string currentEmail,
        string newEmail)
    {
        if (userId == null || code == null)
        {
            _logger.LogWarning("Failed to confirm user email reset - code or user Id was null");

            return View("Error");
        }

        var userResult = await _userService.FindByIdAsync(userId);

        if (!userResult.IsSuccessful)
        {
            _logger.LogError("Failed to confirm email reset for user {userId}! Error: {errorMessage}",
                userId, userResult.Message);

            TempData.TempDataMessage("Error", $"{userResult.Message}");

            return RedirectToAction("Login", "Account");
        }

        var user = userResult.Data;
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var updateResult = await _userService.UpdateEmailAsync(currentEmail, newEmail);

            if (!updateResult.IsSuccessful)
            {
                _logger.LogError("Failed to reset email for user {userId}! Error: {errorMessage}",
                    userId, updateResult.Message);

                TempData.TempDataMessage("Error", $"{updateResult.Message}");

                return View("Login");
            }

            return RedirectToAction("Logout");
        }
        else
        {
            _logger.LogError("Failed to confirm email reset for user {userId}!", userId);

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Error: {errorMessage}", error.Description);
            }

            TempData.TempDataMessage("Error", $"Failed to confirm email");

            return View("Error");
        }
    }
}