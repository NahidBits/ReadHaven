using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Book;
using ReadHaven.Services;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")] 
    public class ProfileController : BaseController
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetUserProfile")]
        public IActionResult GetUserProfile()
        {
            var user = _context.Users
                .Where(u => u.Id == UserId).FirstOrDefault();

            var userProfile = new
            {
                Name = user.Username,
                Email = user.Email
            };

            return Ok(userProfile);
        }
    }

}
