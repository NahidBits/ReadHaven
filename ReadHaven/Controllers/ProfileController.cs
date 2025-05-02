using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;
using ReadHaven.ViewModels;
using ReadHaven.Services;
using ReadHaven.Models.Book;
using System.Linq;

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
                .Where(u => u.Id == UserId)
                .Select(u => new
                {
                    Name = u.Username,
                    Email = u.Email
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("LoadProfileSection")]
        public IActionResult LoadProfileSection()
        {
            return PartialView("_ProfileSection");
        }

        [HttpGet("LoadMyOrderSection")]
        public IActionResult LoadMyOrderSection()
        {
            return PartialView("_MyOrderSection");
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("LoadUserOrderSection")]
        public IActionResult LoadUserOrderSection()
        {
            return PartialView("_UserOrderSection");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("LoadBookSalesSection")]
        public IActionResult LoadBookSalesSection()
        {
            return PartialView("_BookSalesSection");
        }
    }
}
