using BestMusPortal.Data.Interfaces;
using BestMusPortal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository, IGenreRepository genreRepository, ISongRepository songRepository)
        {
            _context = context;
            Users = userRepository;
            Genres = genreRepository;
            Songs = songRepository;
        }

        public IUserRepository Users { get; private set; }
        public IGenreRepository Genres { get; private set; }
        public ISongRepository Songs { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
