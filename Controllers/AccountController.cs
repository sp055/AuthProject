using AuthProject.Interfaces;
using AuthProject.Models;
using AuthProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace AuthProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISendGridEmail _sendGridEmail;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ISendGridEmail sendGridEmail, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendGridEmail = sendGridEmail;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            LogInViewModel logInViewModel = new LogInViewModel();
            logInViewModel.ReturnUrl = returnUrl ?? Url.Content("~/");
            return View(logInViewModel);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackurl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                await _sendGridEmail.SendEmailAsync(model.Email, "Reset Email Confitmation", "Please reset email by going to this link <a href=\"" + callbackurl + "\">link</a>");
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogInViewModel logInViewModel, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(logInViewModel.UserName, logInViewModel.Password, logInViewModel.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(logInViewModel);
                }
            }
            return View(logInViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
                await _roleManager.CreateAsync(new IdentityRole("ADMIN"));
            }
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "ADMIN",
                Text = "ADMIN"
            });
            listItems.Add(new SelectListItem()
            {
                Value = "User",
                Text = "User"
            });
            RegisterViewModel registerViewModel = new RegisterViewModel();
            registerViewModel.RoleList = listItems;
            registerViewModel.ReturnUrl = returnUrl;
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new AppUser { Email = registerViewModel.Email, UserName = registerViewModel.UserName };
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    if (registerViewModel.RoleSelected != null && registerViewModel.RoleSelected.Length > 0 && registerViewModel.RoleSelected == "ADMIN")
                    {
                        await _userManager.AddToRoleAsync(user, "ADMIN");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                ModelState.AddModelError("Password", "User could not be created. Password not unique enough");
            }
            return View(registerViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel editViewModel, string? returnUrl = null)
        {
            editViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            var user = await _userManager.FindByEmailAsync(editViewModel.Email);
            if (user != null)
            {
                //var newUser = new AppUser() {};
                //var updateUser = await _userManager.UpdateAsync(user, editViewModel.NewPassword);

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, editViewModel.NewPassword);
            }

            return View(editViewModel);
        }
    }
}
