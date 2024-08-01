using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BestMusPortal.Data;
using BestMusPortal.Models;
using AutoMapper;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;

namespace MusicPortalLaLaFa.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(IUserService userService, IMapper mapper, ILogger<RegistrationController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: /Registration/RegisterAdmin
        public async Task<IActionResult> RegisterAdmin()
        {
            if ((await _userService.GetAllUsersAsync()).Any(u => u.Role == "Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(UserDTO userDto)
        {
            if (ModelState.IsValid)
            {
                userDto.Role = "Admin";
                userDto.IsApproved = true;
                userDto.IsActive = true;

                await _userService.AddUserAsync(userDto);
                _logger.LogInformation("Admin user added.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Validation error in field {Field}: {Error}", state.Key, error.ErrorMessage);
                    }
                }
            }
            return View(userDto);
        }

        // GET: /Registration/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserDTO userDto)
        {
            if (ModelState.IsValid)
            {
                userDto.Role = "User";
                userDto.IsApproved = false;
                userDto.IsActive = false;

                await _userService.AddUserAsync(userDto);
                return RedirectToAction("Index", "Home");
            }
            return View(userDto);
        }
    }
}