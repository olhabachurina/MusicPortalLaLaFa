using AutoMapper;
using BestMusPortal.Models;
using BestMusPortal.Services;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MusicPortalLaLaFa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISongService _songService;
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ISongService songService, IMapper mapper, ILogger<HomeController> logger)
        {
            _songService = songService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all songs for the home page.");
            var songs = await _songService.GetAllSongsAsync();
            var songDtos = _mapper.Map<IEnumerable<SongDTO>>(songs);
            var lastSong = songDtos.OrderByDescending(s => s.SongId).FirstOrDefault();

            if (lastSong != null)
            {
                ViewBag.LastSongUrl = Url.Content($"~/{lastSong.VideoFilePath}");
            }
            else
            {
                _logger.LogWarning("No songs found.");
            }

            return View(songDtos);
        }
    }
}