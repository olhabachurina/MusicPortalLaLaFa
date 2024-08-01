using BestMusPortal.Models;
using BestMusPortal.Services.DTO;

namespace BestMusPortal.Services.Interfaces
{
    public interface ISongService
    {
        Task<IEnumerable<SongDTO>> GetAllSongsAsync();
        Task<SongDTO> GetSongByIdAsync(int songId);
        Task AddSongAsync(SongDTO songDTO);
        Task UpdateSongAsync(SongDTO songDTO);
        Task DeleteSongAsync(int songId);
    }
}
