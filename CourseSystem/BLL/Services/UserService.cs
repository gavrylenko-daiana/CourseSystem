using System.Security.Claims;
using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;

public class UserService : GenericService<AppUser>, IUserService
{
    private readonly UserManager<AppUser> _userManager;
    
    protected UserService(UnitOfWork unitOfWork, UserManager<AppUser> userManager) : base(unitOfWork, unitOfWork.UserRepository)
    {
        _userManager = userManager;
    }
    
}