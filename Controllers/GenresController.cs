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
using Microsoft.AspNetCore.Authorization;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using MusicPortalLaLaFa.Models;
using System.ComponentModel.DataAnnotations;

namespace MusicPortalLaLaFa.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class GenresController : Controller
    {
        private readonly IGenreService _genreService;
        private readonly ILogger<GenresController> _logger;

        public GenresController(IGenreService genreService, ILogger<GenresController> logger)
        {
            _genreService = genreService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all genres.");
            var genres = await _genreService.GetAllGenresAsync();
            var genreViewModels = genres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name
            }).ToList();
            return View(genreViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] GenreViewModel genreViewModel)
        {
            if (ModelState.IsValid)
            {
                var genreDto = new GenreDTO
                {
                    Name = genreViewModel.Name
                };
                await _genreService.AddGenreAsync(genreDto);
                _logger.LogInformation("Genre created successfully.");
                return RedirectToAction(nameof(Index));
            }
            return View(genreViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            var genreViewModel = new GenreViewModel
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            };

            return View(genreViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GenreId,Name")] GenreViewModel genreViewModel)
        {
            if (id != genreViewModel.GenreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var genreDto = new GenreDTO
                {
                    GenreId = genreViewModel.GenreId,
                    Name = genreViewModel.Name
                };

                try
                {
                    await _genreService.UpdateGenreAsync(genreDto);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await GenreExists(genreDto.GenreId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _logger.LogInformation("Genre updated successfully.");
                return RedirectToAction(nameof(Index));
            }
            return View(genreViewModel);
        }
        private async Task<bool> GenreExists(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            return genre != null;
        }
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
            {
                TempData["Error"] = "Жанр не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(new GenreViewModel
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Неверный идентификатор жанра.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _genreService.DeleteGenreAsync(id);
                TempData["Success"] = "Жанр успешно удален.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ошибка при удалении жанра: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}