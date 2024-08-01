using BestMusPortal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByNameAsync(string userName);
    }
}
