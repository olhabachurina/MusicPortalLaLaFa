using AutoMapper;
using BestMusPortal.Data;
using BestMusPortal.Data.Interfaces;
using BestMusPortal.Models;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using BestMusPortal.Services.Infrastructure;
using BestMusPortal.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Services.Services
{
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GenreService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GenreDTO>> GetAllGenresAsync()
        {
            var genres = await _unitOfWork.Genres.GetAllAsync();
            return _mapper.Map<IEnumerable<GenreDTO>>(genres);
        }

        public async Task<GenreDTO> GetGenreByIdAsync(int genreId)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
            return _mapper.Map<GenreDTO>(genre);
        }

        public async Task AddGenreAsync(GenreDTO genreDto)
        {
            var genre = _mapper.Map<Genre>(genreDto);
            await _unitOfWork.Genres.CreateAsync(genre);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateGenreAsync(GenreDTO genreDto)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreDto.GenreId);
            if (genre == null)
            {
                throw new ValidationException("Genre not found", nameof(genreDto.GenreId));
            }

            _mapper.Map(genreDto, genre);
            await _unitOfWork.Genres.UpdateAsync(genre);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteGenreAsync(int genreId)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
            if (genre == null)
            {
                throw new ValidationException("Genre not found", nameof(genreId));
            }

            await _unitOfWork.Genres.DeleteAsync(genreId);
            await _unitOfWork.SaveAsync();
        }
    }
}