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
using Microsoft.AspNetCore.SignalR;
using MusicPortalLaLaFa.Hubs;

namespace MusicPortalLaLaFa.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class GenresController : Controller
    {
        private readonly IGenreService _genreService;
        private readonly ILogger<GenresController> _logger;
        private readonly IHubContext<MusicHub> _hubContext; // Добавляем IHubContext

        public GenresController(IGenreService genreService, ILogger<GenresController> logger, IHubContext<MusicHub> hubContext)
        {
            _genreService = genreService;
            _logger = logger;
            _hubContext = hubContext; // Инициализируем IHubContext
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all genres.");
            var genres = await _genreService.GetAllGenresAsync();

            // Преобразуем список GenreDTO в GenreViewModel, если в дальнейшем планируется использовать ViewModel
            var genreDtos = genres.Select(g => new GenreDTO
            {
                GenreId = g.GenreId,
                Name = g.Name
            }).ToList();

            return View(genreDtos);  // Передаем список GenreDTO в представление
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

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Genre '{genreDto.Name}' has been created.", "genre");

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

                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Genre '{genreDto.Name}' has been updated.", "genre");
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

        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
            {
                TempData["Error"] = "Genre not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _genreService.DeleteGenreAsync(id);

                // Уведомляем клиентов через SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Genre '{genre.Name}' has been deleted.", "genre");

                _logger.LogInformation("Жанр успешно удален.");

                // Перенаправляем на страницу со списком жанров
                TempData["Success"] = "Genre deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при удалении жанра: {Message}", ex.Message);
                TempData["Error"] = "Ошибка при удалении жанра.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {  
            if (id <= 0)
            {
                TempData["Error"] = "Invalid genre ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var genre = await _genreService.GetGenreByIdAsync(id);
                if (genre == null)
                {
                    TempData["Error"] = "Genre not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _genreService.DeleteGenreAsync(id);

                // Уведомляем клиентов через SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Genre '{genre.Name}' has been deleted.", "genre");

                TempData["Success"] = "Genre deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting genre with ID {GenreId}", id);
                TempData["Error"] = "An error occurred while deleting the genre: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> GenreExists(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            return genre != null;
        }
    }
}
