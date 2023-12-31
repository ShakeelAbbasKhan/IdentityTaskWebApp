﻿using IdentityTaskWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdentityTaskWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;


        public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> userManger,
            SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _db = db;
            _userManager = userManger;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                   // var user = await _userManager.FindByNameAsync(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid Login Attempt");

            }
            return View(model);
        }
        //[Authorize(Roles = "Admin")]
        [Authorize(Policy = "SuperUserRights")]
        public async Task<IActionResult> Register()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var model = new RegisterViewModel
            {
                Roles = roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList(),
            };

            return View(model);

        }
        //[Authorize(Roles = "Admin")]

        [Authorize(Policy = "SuperUserRights")]  // by policy

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(string? id, RegisterViewModel registerModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(id))
                {
                    var user = new ApplicationUser
                    {
                        UserName = registerModel.Email,
                        Email = registerModel.Email,
                        FirstName = registerModel.FirstName,
                        LastName = registerModel.LastName,
                    };

                    var result = await _userManager.CreateAsync(user, registerModel.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, registerModel.RoleName);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {

                    var user = await _userManager.FindByIdAsync(id);

                    if (user != null)
                    {
                        var passwordhash =   new PasswordHasher<ApplicationUser>();
                        string hashedpassword = passwordhash.HashPassword(user, registerModel.Password); 

                        user.UserName = registerModel.Email;
                        user.Email = registerModel.Email;
                        user.FirstName = registerModel.FirstName;
                        user.LastName = registerModel.LastName;
                        user.PasswordHash= hashedpassword;

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            //await _userManager.AddToRoleAsync(user, registerModel.RoleName);
                            //await _signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction(nameof(UserList));
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }

                }


            }
            return View();
        }
        // [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName
        //        };
        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await _userManager.AddToRoleAsync(user, model.RoleName);

        //                await _signInManager.SignInAsync(user, isPersistent: false);   // if account has been created then we can automatically sign in to new user

        //            return RedirectToAction("Index", "Home");
        //        }
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }
        //    }
        //    return View(model);
        //}


        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult UserList()
        {
            //var users = _userManager.Users.ToList(); 

            //return View(users);

            var users = _userManager.Users.Select(u => new UserListVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Roles = _userManager.GetRolesAsync(u).Result.ToList()
            }).ToList();

            return View(users);
        }


        public async Task<IActionResult> Edit(string id)
        {
            var user = _userManager.FindByIdAsync(id).Result;


            if (user != null)
            {
                var resultuser = new UserEdit
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
                return View(resultuser);
            }
            return RedirectToAction("Index");
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(ApplicationUser model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByIdAsync(model.Id);
        //        if (user != null)
        //        {
        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            var result = await _userManager.UpdateAsync(user);
        //            if (result.Succeeded)
        //            {
        //                return RedirectToAction("UserList");
        //            }

        //            foreach (var error in result.Errors)
        //            {
        //                ModelState.AddModelError("", error.Description);
        //            }
        //        }
        //    }

        //    return View(model); 
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserList");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction("UserList");
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
                if (user != null )
                    //&& await _userManager.IsEmailConfirmedAsync(user)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token = token }, Request.Scheme);

                    ViewBag.Url = passwordResetLink;

                    _logger.Log(LogLevel.Warning, passwordResetLink);

                    return View("ForgotPasswordConfirmation");
                }
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                return View("ResetPasswordConfirmation");
            }
            return View(model);
        }


    }
}
