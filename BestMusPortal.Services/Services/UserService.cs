using AutoMapper;
using BestMusPortal.Data.Interfaces;
using BestMusPortal.Data.Repositories;
using BestMusPortal.Models;
using BestMusPortal.Services.DTO;
using BestMusPortal.Services.Interfaces;
using System;
using System.Collections.Generic;
using BestMusPortal.Services.Infrastructure;
using System.ComponentModel.DataAnnotations;
using BestMusPortal.Services.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestMusPortal.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(users);
    }

    public async Task<UserDTO> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO> GetUserByNameAsync(string userName)
    {
        var user = await _unitOfWork.Users.GetUserByNameAsync(userName);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task AddUserAsync(UserDTO userDto)
    {
        var user = _mapper.Map<User>(userDto);
        await _unitOfWork.Users.CreateAsync(user);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateUserAsync(UserDTO userDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userDto.UserId);
        if (user == null)
        {
            throw new BestMusPortal.Services.Infrastructure.ValidationException("User not found", nameof(userDto.UserId));
        }

        _mapper.Map(userDto, user);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}
}