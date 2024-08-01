using AutoMapper;
using BestMusPortal.Data.Interfaces;
using BestMusPortal.Data.Repositories;
using BestMusPortal.Models;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using BestMusPortal.Services.Infrastructure;
using BestMusPortal.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Services.Services
{
    public class SongService : ISongService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SongService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SongDTO>> GetAllSongsAsync()
        {
            var songs = await _unitOfWork.Songs.GetAllAsync();
            return _mapper.Map<IEnumerable<SongDTO>>(songs);
        }

        public async Task<SongDTO> GetSongByIdAsync(int songId)
        {
            var song = await _unitOfWork.Songs.GetByIdAsync(songId);
            return _mapper.Map<SongDTO>(song);
        }

        public async Task AddSongAsync(SongDTO songDto)
        {
            if (songDto == null)
            {
                throw new ArgumentNullException(nameof(songDto));
            }

            // Убедитесь, что Genre заполнено
            if (string.IsNullOrEmpty(songDto.Genre))
            {
                throw new InvalidOperationException("Genre cannot be null or empty.");
            }

            // Маппинг SongDTO на Song
            var song = _mapper.Map<Song>(songDto);

            // Добавление песни в базу данных
            await _unitOfWork.Songs.CreateAsync(song);
        }

        public async Task UpdateSongAsync(SongDTO songDto)
        {
            var song = await _unitOfWork.Songs.GetByIdAsync(songDto.SongId);
            if (song == null)
            {
                throw new ValidationException("Song not found", nameof(songDto.SongId));
            }

            _mapper.Map(songDto, song);
            await _unitOfWork.Songs.UpdateAsync(song);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteSongAsync(int songId)
        {
            var song = await _unitOfWork.Songs.GetByIdAsync(songId);
            if (song == null)
            {
                throw new ValidationException("Song not found", nameof(songId));
            }

            await _unitOfWork.Songs.DeleteAsync(songId);
            await _unitOfWork.SaveAsync();
        }
    }
}