using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BestMusPortal.Data;
using BestMusPortal.Models;
using BestMusPortal.Services;
using BestMusPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MusicPortalLaLaFa.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserService _userService;

        public AdminController(ILogger<AdminController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            _logger.LogInformation("Users fetched: {Count}", users.Count());
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userDto = await _userService.GetUserByIdAsync(id);
            if (userDto != null)
            {
                userDto.IsApproved = true;
                userDto.IsActive = true;
                await _userService.UpdateUserAsync(userDto);
                _logger.LogInformation("User approved: {UserName}", userDto.UserName);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userDto = await _userService.GetUserByIdAsync(id);
            if (userDto != null)
            {
                await _userService.DeleteUserAsync(id);
                _logger.LogInformation("User rejected and removed: {UserName}", userDto.UserName);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AllUsers(string nameFilter, string emailFilter, string statusFilter)
        {
            var users = await _userService.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                users = users.Where(u => u.UserName.Contains(nameFilter));
            }

            if (!string.IsNullOrEmpty(emailFilter))
            {
                users = users.Where(u => u.Email.Contains(emailFilter));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter.ToLower() == "active")
                {
                    users = users.Where(u => u.IsActive);
                }
                else if (statusFilter.ToLower() == "inactive")
                {
                    users = users.Where(u => !u.IsActive);
                }
            }

            return PartialView("~/Views/Admin/_UserListPartial.cshtml", users.ToList());
        }
    }
}