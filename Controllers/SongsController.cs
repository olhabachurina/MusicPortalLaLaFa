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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Diagnostics;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using System.Xml.Linq;
using BestMusPortal.Services.Services;
using BestMusPortal.Services.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MusicPortalLaLaFa.Hubs;
namespace MusicPortalLaLaFa.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class SongsController : Controller
    {
        private readonly ISongService _songService;
        private readonly IGenreService _genreService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<SongsController> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<MusicHub> _hubContext;
        public SongsController(
    ISongService songService,
    IGenreService genreService,
    IUserService userService,
    IWebHostEnvironment webHostEnvironment,
    ILogger<SongsController> logger,
    IMapper mapper,
    IHubContext<MusicHub> hubContext // Добавляем IHubContext
)
        {
            _songService = songService;
            _genreService = genreService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext; // Инициализируем поле _hubContext
        }

        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync();
            var songDTOs = songs.Select(song => _mapper.Map<SongDTO>(song)).ToList();

            SetLastSongUrlInViewBag(songDTOs);
            return View(songDTOs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Starting Create (GET) method.");

            var userName = User.Identity?.Name;
            var user = await GetUserByNameAsync(userName);
            if (user == null) return BadRequest("User is not authenticated");

            await SetGenresAndMoodsInViewBag();

            var model = new SongDTO
            {
                UserId = user.UserId,
                UserName = user.UserName
            };

            _logger.LogInformation("Ending Create (GET) method.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SongDTO songDTO)
        {
            _logger.LogInformation("Creating song: {Title}", songDTO.Title);

            var userName = User.Identity?.Name;
            var user = await GetUserByNameAsync(userName);
            if (user == null) return BadRequest("User is not authenticated");

            songDTO.MusicFilePath = await SaveFileAsync(songDTO.MusicFile, "music");
            songDTO.VideoFilePath = await SaveFileAsync(songDTO.VideoFile, "videos");
            songDTO.VideoUrl = Url.Content($"~/{songDTO.VideoFilePath}");
            songDTO.UserName = user.UserName;

            var genre = await _genreService.GetGenreByIdAsync(songDTO.GenreId);

            if (genre == null)
            {
                ModelState.AddModelError("Genre", "Invalid Genre selected.");
                await SetGenresAndMoodsInViewBag();
                return View(songDTO);
            }

            songDTO.Genre = genre.Name;

            if (string.IsNullOrEmpty(songDTO.Genre))
            {
                _logger.LogError("Genre is null or empty before saving.");
                ModelState.AddModelError("Genre", "Genre cannot be null or empty.");
                await SetGenresAndMoodsInViewBag();
                return View(songDTO);
            }

            await _songService.AddSongAsync(songDTO);

            // Отправка уведомления через SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"A new song '{songDTO.Title}' has been added.");

            TempData["Success"] = "Песня успешно создана!";
            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            _logger.LogInformation("Displaying Success page.");
            ViewBag.SuccessMessage = TempData["Success"];
            ViewBag.MusicFilePath = TempData["MusicFilePath"];
            ViewBag.VideoFilePath = TempData["VideoFilePath"];
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Starting Edit (GET) method for song ID: {Id}", id);

            var song = await _songService.GetSongByIdAsync(id);
            if (song == null)
            {
                _logger.LogWarning("Song with ID {Id} not found.", id);
                return NotFound("Song not found");
            }

            var userName = User.Identity?.Name;
            var user = await GetUserByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User is not authenticated");
                return BadRequest("User is not authenticated");
            }

            var model = _mapper.Map<SongDTO>(song);
            model.UserName = user.UserName;

            await SetGenresAndMoodsInViewBag();

            _logger.LogInformation("Ending Edit (GET) method for song ID: {Id}", id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SongDTO songDTO)
        {
            _logger.LogInformation("Editing song with ID: {Id}", id);

            if (id != songDTO.SongId)
            {
                _logger.LogWarning("Song ID in the URL does not match the Song ID in the form.");
                return BadRequest("Invalid song ID");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for song ID: {Id}", id);
                await SetGenresAndMoodsInViewBag();
                return View(songDTO);
            }

            // Получаем текущего пользователя
            var userName = User.Identity?.Name;
            var user = await GetUserByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User is not authenticated");
                return BadRequest("User is not authenticated");
            }

            // Обновление файлов
            if (songDTO.MusicFile != null)
            {
                songDTO.MusicFilePath = await SaveFileAsync(songDTO.MusicFile, "music");
            }
            if (songDTO.VideoFile != null)
            {
                songDTO.VideoFilePath = await SaveFileAsync(songDTO.VideoFile, "videos");
                songDTO.VideoUrl = Url.Content($"~/{songDTO.VideoFilePath}");
            }

            // Получение жанра на основе GenreId
            var genre = await _genreService.GetGenreByIdAsync(songDTO.GenreId);

            if (genre == null)
            {
                _logger.LogError("Invalid Genre selected for song ID: {Id}", id);
                ModelState.AddModelError("Genre", "Invalid Genre selected.");
                await SetGenresAndMoodsInViewBag();
                return View(songDTO);
            }

            // Присваиваем имя жанра в поле Genre объекта songDTO
            songDTO.Genre = genre.Name;

            // Логирование для проверки заполнения поля Genre
            _logger.LogInformation("Genre: {Genre} for song ID: {Id}", songDTO.Genre, id);

            // Проверяем, заполнено ли поле Genre
            if (string.IsNullOrEmpty(songDTO.Genre))
            {
                _logger.LogError("Genre is null or empty before saving for song ID: {Id}", id);
                ModelState.AddModelError("Genre", "Genre cannot be null or empty.");
                await SetGenresAndMoodsInViewBag();
                return View(songDTO);
            }

            // Маппинг DTO обратно на сущность и сохранение
            var song = await _songService.GetSongByIdAsync(id);
            if (song == null)
            {
                _logger.LogError("Song with ID {Id} not found during update.", id);
                return NotFound("Song not found");
            }

            _mapper.Map(songDTO, song);

            await _songService.UpdateSongAsync(song);

            TempData["Success"] = "Песня успешно обновлена!";
            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var song = await _songService.GetSongByIdAsync(id);
            if (song == null) return NotFound();

            var model = _mapper.Map<SongDTO>(song);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _songService.GetSongByIdAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            await _songService.DeleteSongAsync(id);

            // Отправка уведомления через SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"The song '{song.Title}' has been deleted.");

            return RedirectToAction(nameof(Index));
        }

        private void SetLastSongUrlInViewBag(IEnumerable<SongDTO> songDTOs)
        {
            var lastSong = songDTOs.OrderByDescending(s => s.SongId).FirstOrDefault();
            if (lastSong != null)
            {
                ViewBag.LastSongUrl = Url.Content($"~/{lastSong.VideoFilePath}");
            }
        }

        private async Task SetGenresAndMoodsInViewBag()
        {
            var genres = await _genreService.GetAllGenresAsync();
            ViewBag.Genres = new SelectList(genres, "GenreId", "Name");

            ViewBag.MoodList = new List<SelectListItem>
        {
            new SelectListItem { Value = "Happy", Text = "Happy" },
            new SelectListItem { Value = "Sad", Text = "Sad" },
            new SelectListItem { Value = "Relaxed", Text = "Relaxed" },
            new SelectListItem { Value = "Energetic", Text = "Energetic" }
        };
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null) return null;

            var directory = Path.Combine(_webHostEnvironment.WebRootPath, $"uploads/{folder}");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var filePath = Path.Combine($"uploads/{folder}", $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        private async Task<UserDTO> GetUserByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogError("User is not authenticated.");
                return null;
            }

            var user = await _userService.GetUserByNameAsync(userName);
            if (user == null)
            {
                _logger.LogError("User not found.");
                return null;
            }

            return user;
        }
        

        [HttpGet]
        public async Task<IActionResult> FilterSongs(string genreFilter, string artistFilter, string moodFilter, string titleFilter)
        {
            Trace.WriteLine($"Received filters - Genre: {genreFilter}, Artist: {artistFilter}, Mood: {moodFilter}, Title: {titleFilter}");

            var songs = await _songService.GetAllSongsAsync();

            Trace.WriteLine($"Initial song count: {songs.Count()}");

            if (!string.IsNullOrEmpty(genreFilter) && genreFilter.ToLower() != "all")
            {
                songs = songs.Where(s => s.Genre != null && s.Genre.Equals(genreFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                Trace.WriteLine($"Songs after genre filter ({genreFilter}): {songs.Count()}");
            }

            if (!string.IsNullOrEmpty(artistFilter))
            {
                Trace.WriteLine($"Artist filter before processing: {artistFilter}");
                songs = songs.Where(s => s.Artist != null && s.Artist.Contains(artistFilter.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                Trace.WriteLine($"Songs after artist filter ({artistFilter}): {songs.Count()}");
            }

            if (!string.IsNullOrEmpty(moodFilter) && moodFilter.ToLower() != "all")
            {
                songs = songs.Where(s => s.Mood != null && s.Mood.Equals(moodFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                Trace.WriteLine($"Songs after mood filter ({moodFilter}): {songs.Count()}");
            }

            if (!string.IsNullOrEmpty(titleFilter))
            {
                songs = songs.Where(s => s.Title != null && s.Title.Contains(titleFilter.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                Trace.WriteLine($"Songs after title filter ({titleFilter}): {songs.Count()}");
            }

            // Преобразуем SongDTO в SongViewModel
            var songViewModels = songs.Select(song => new Models.SongViewModel
            {
                SongId = song.SongId,
                Title = song.Title,
                Artist = song.Artist,
                GenreId = song.GenreId,
                Genre = song.Genre,
                Mood = song.Mood,
                MusicFilePath = song.MusicFilePath,
                VideoFilePath = song.VideoFilePath,
                UserId = song.UserId,
                UserName = song.UserName,
                VideoUrl = song.VideoUrl
            }).ToList();

            Trace.WriteLine($"Final song count: {songViewModels.Count}");

            return PartialView("_SongListPartial", songViewModels);
        }
    }
}