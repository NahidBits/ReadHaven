using Microsoft.AspNetCore.Http;
using ReadHaven.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Services;
using ReadHaven;
using System.Security.Claims;

public interface IUserService
{
    Task<string> GetCurrentUserRole();
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> GetCurrentUserRole()
    {
        var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);

        var user = await _context.Users
                                  .FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user != null)
        {
            var userRole = await _context.UserRoles
                                          .FirstOrDefaultAsync(u => u.UserId == user.Id);

            if (userRole != null)
            {
                return userRole.Role;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}
