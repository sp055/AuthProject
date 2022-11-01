﻿using System.Security.Claims;
using AuthProject.Data;
using AuthProject.Models;
using AuthProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
        }
        // public IActionResult Index()
        // {
        //     return View();
        // }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            LogInViewModel logInViewModel = new LogInViewModel();
            logInViewModel.ReturnUrl = returnUrl ?? Url.Content("~/");
            return View(logInViewModel);
        }

        // [HttpGet]
        // public IActionResult ForgotPassword()
        // {
        //     return View();
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogInViewModel logInViewModel, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(logInViewModel.UserName, logInViewModel.Password,
                    logInViewModel.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(logInViewModel.UserName);

                    if (user.FirstLogin)
                    {
                        return RedirectToAction("Edit", "Account");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (result.IsLockedOut)
                {
                    return View();
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
        [Authorize(Roles = "ADMIN")]
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
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    Email = registerViewModel.Email, UserName = registerViewModel.UserName,
                    LastPasswChange = DateTime.Now
                };
                
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    if (registerViewModel.RoleSelected != null && registerViewModel.RoleSelected.Length > 0 &&
                        registerViewModel.RoleSelected == "ADMIN")
                    {
                        await _userManager.AddToRoleAsync(user, "ADMIN");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    //await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "User");
                }

                ModelState.AddModelError("Password", "User could not be created. Password not unique enough");
            }

            return View(registerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,User")]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        // [HttpGet]
        // public IActionResult ForgotPasswordConfirmation()
        // {
        //     return View();
        // }

        [HttpGet]
        [Authorize(Roles = "ADMIN,User")]
        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,User")]
        public async Task<IActionResult> Edit(EditViewModel editViewModel, string? returnUrl = null)
        {
            editViewModel.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claimsEmail = claimsIdentity.FindFirst(ClaimTypes.Email).Value;

            var user = await _userManager.FindByEmailAsync(claimsEmail);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, editViewModel.OldPassword, false);
                if (result.Succeeded)
                {
                    var updatedUser = new AppUser
                    {
                        RoleId = user.RoleId,
                        Role = user.Role,
                        RoleList = user.RoleList,
                        FirstLogin = false,
                        LastPasswChange = DateTime.Now
                    };

                    await _userManager.UpdateAsync(updatedUser);
                    _db.SaveChanges();

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, editViewModel.NewPassword);
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(editViewModel);
        }
    }
}