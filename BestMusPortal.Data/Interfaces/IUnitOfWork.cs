using BestMusPortal.Data.Repositories;
using BestMusPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGenreRepository Genres { get; }
        ISongRepository Songs { get; }
        Task<int> CompleteAsync();
        Task SaveAsync();
        void Dispose();
    }
}
