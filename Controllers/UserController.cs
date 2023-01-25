using AuthProject.Data;
using AuthProject.Models;
using AuthProject.Models;
using AuthProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;

namespace AuthProject.Controllers
{
    [ValidateReCaptcha]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userList = _db.AppUser.ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            //set user to none to not make ui look terrible
            foreach (var user in userList)
            {
                var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
                if (role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId).Name;
                }
            }

            return View(userList);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
            if (role != null)
            {
                user.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }

            user.RoleList = _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(AppUser user)
        {
            if (ModelState.IsValid)
            {
                var userDbValue = _db.AppUser.FirstOrDefault(u => u.Id == user.Id);
                if (userDbValue == null)
                {
                    return NotFound();
                }

                var userRole = _db.UserRoles.FirstOrDefault(u => u.UserId == userDbValue.Id);
                if (userRole != null)
                {
                    var previousRoleName = _db.Roles.Where(u => u.Id == userRole.RoleId).Select(e => e.Name)
                        .FirstOrDefault();
                    await _userManager.RemoveFromRoleAsync(userDbValue, previousRoleName);
                }

                await _userManager.AddToRoleAsync(userDbValue, _db.Roles.FirstOrDefault(u => u.Id == user.RoleId).Name);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }


            user.RoleList = _db.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePasswd(string userId) //TODO name refactor
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
            if (role != null)
            {
                user.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }

            user.RoleList = _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangePasswd(AppUser changePasswdAdminViewModel)
        {
            if (ModelState.IsValid)
            {
                return View(changePasswdAdminViewModel);
            }

            var user = await _userManager.FindByIdAsync(changePasswdAdminViewModel.Id);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, changePasswdAdminViewModel.PasswordAm);
                return RedirectToAction("Index", "Home");
            }

            return View(changePasswdAdminViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockAccount(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
            _db.SaveChanges();

            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockAccount(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            _db.SaveChanges();

            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteAccount(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            _db.Remove(user);
            _db.SaveChanges();

            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult TogglePasswordCheck()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult PasswordCheckOff()
        {
            TogglePasswordCheckViewModel.PasswordCheck = false;
            _db.SaveChanges();

            return RedirectToAction("TogglePasswordCheck", "User");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult PasswordCheckOn()
        {
            TogglePasswordCheckViewModel.PasswordCheck = true;
            _db.SaveChanges();

            return RedirectToAction("TogglePasswordCheck", "User");
        }



    }
}