using BestMusPortal.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Services.Interfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreDTO>> GetAllGenresAsync();
        Task<GenreDTO> GetGenreByIdAsync(int genreId);
        Task AddGenreAsync(GenreDTO genreDTO);
        Task UpdateGenreAsync(GenreDTO genreDTO);
        Task DeleteGenreAsync(int genreId);
    }
}