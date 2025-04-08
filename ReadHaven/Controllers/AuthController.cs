using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;        

namespace ReadHaven.Controllers
{
    //[Route("Auth")]
    public class AuthController : Controller
    {
        public readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }

         [HttpPost]
        // [HttpPost("Login")]
       // [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findUser != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(findUser, findUser.PasswordHash, user.PasswordHash);

                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                 {
                    new Claim(ClaimTypes.Email, findUser.Email),
                    new Claim(ClaimTypes.Role, "User")
                 };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                   new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Book");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        //[HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

      //  [HttpPost("{Controller}/SignUp")]
        //[ValidateAntiForgeryToken]
       // [HttpPost]
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

                if (findUser != null)
                {
                    return Json(new { success = false, message = "Email is already used." });
                }

                // Check if the password matches the confirm password
                if (user.PasswordHash != confirmPassword)
                {
                    return Json(new { success = false, message = "Passwords do not match." });
                }

                // Hash the password before saving
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Return success and redirect URL
                return RedirectToAction("Index", "Auth", new { returnUrl = HttpContext.Request.Path });
            }

            return View();
        }
        /*

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Auth");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("{Controller}/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] User user)
        {
            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findUser != null)
                return Json(new { success = true, message = $"Your passwword is : {findUser.PasswordHash}" });
            else
                return Json(new { success = false, message = "There is no user" });
        }*/
    }
}
