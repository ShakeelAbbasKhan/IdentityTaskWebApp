﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IdentityTaskWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;

[Authorize(policy: "SuperUserRights")]
public class RoleController : Controller
{
    
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {

        var roles = _roleManager.Roles.ToList();
        var roleViewModels = new List<RoleViewModel>();

        foreach (var role in roles)
        {
            var roleViewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                AssignedRoleCount = (await _userManager.GetUsersInRoleAsync(role.Name)).Count
            };

            roleViewModels.Add(roleViewModel);
        }

        return View(roleViewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(IdentityRole model)
    {
        if (!string.IsNullOrEmpty(model.Name))
        {
            var role = new IdentityRole { Name = model.Name };
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, IdentityRole model)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        role.Name = model.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        if (usersInRole.Any())
        {
            ModelState.AddModelError("", "This role cannot be deleted because it is assigned to one or more users.");
            return View(role);
        }

        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            return RedirectToAction("Index");
        }

        return View(role);
    }


    public async Task<IActionResult> AssignRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        var allRoles = _roleManager.Roles.ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new AssignRolesViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email,
            Roles = allRoles.Select(role => new UserRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsSelected = userRoles.Contains(role.Name)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRoles(AssignRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, userRoles);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles);

        if (result.Succeeded)
        {
            return RedirectToAction("UserList","Account");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }


}
