using System.Security.Claims;
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
        private readonly ApplicationDbContext _db;

        private double OTP = new Random().NextDouble();

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
             ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {


            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin"
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.UserName,
                    LastPasswChange = DateTime.Now,
                    FirstLogin = true
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    if (registerViewModel.RoleSelected != null && registerViewModel.RoleSelected.Length > 0 &&
                        registerViewModel.RoleSelected == "Admin")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
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
        [Authorize(Roles = "Admin,User")]
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
        [Authorize(Roles = "Admin,User")]
        public IActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
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
                    user.FirstLogin = false;
                    user.LastPasswChange = DateTime.Now;

                    await _userManager.UpdateAsync(user);
                    _db.SaveChanges();

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, editViewModel.NewPassword);
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(editViewModel);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult UserActivity()
        {
            var userActivity = _db.UserActivities;
            List<UserActivityVM> uaVM = new List<UserActivityVM>();
            foreach (var item in userActivity)
            {
                var temp = new UserActivityVM();
                temp.UserName = item.UserName;
                temp.Url = item.Url;
                temp.Data = item.Data;
                temp.IpAddress = item.IpAddress;
                temp.MethodType = item.MethodType;
                temp.ActivityDate = item.ActivityDate;

                uaVM.Add(temp);
            }

            return View(uaVM);
        }
    }
}