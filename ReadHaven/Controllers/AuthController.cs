using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.User;
using ReadHaven.Services;

namespace ReadHaven.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService; 

        public AuthController(AppDbContext context, IEmailSender emailSender, IConfiguration configuration, IUserService userService)
        {
            _context = context;
            _emailSender = emailSender;
            _configuration = configuration;
            _userService = userService; 
        }

        [HttpGet("Login")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Index(User user)
        {
            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findUser != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(findUser, findUser.PasswordHash, user.PasswordHash);

                if (result == PasswordVerificationResult.Success)
                {
                    var findUserRole = await _context.UserRoles.FirstOrDefaultAsync(u => u.UserId == findUser.Id);

                    if (findUserRole == null)
                        return View();


                    var claims = new List<Claim>
                 {
                    new Claim(ClaimTypes.Email, findUser.Email),
                    new Claim(ClaimTypes.Role, findUserRole.Role)
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

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

                if (findUser != null)
                {
                    return View();
                }

                if (user.PasswordHash != confirmPassword)
                {
                    return View();
                }

                // Hash the password before saving
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);

                _context.Users.Add(user);
                UserRole userRole = new UserRole
                {
                    UserId = user.Id,
                    Role = "User"
                };

                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Auth", new { returnUrl = HttpContext.Request.Path });
            }

            return View();
        }

        [HttpGet("GetUserRoleStatus")]
        public async Task<IActionResult> GetUserRoleStatus()
        {
            var role = await _userService.GetCurrentUserRole();

            if (role != null)
            {
                return Ok(role);
            }

            return NotFound("Role not found");
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return View();
            }

            var token = Guid.NewGuid().ToString();

            // Save the token in the database (you can create a ResetPasswordToken table if necessary)
            var resetToken = new ResetPasswordToken
            {
                UserId = user.Id,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddHours(1) // Token expires in 1 hour
            };
            _context.ResetPasswordToken.Add(resetToken);
            await _context.SaveChangesAsync();

            // Send the reset link via email
            var resetLink = Url.Action("ResetPassword", "Auth", new { token = token }, Request.Scheme);
            var message = $"Click <a href='{resetLink}'>here</a> to reset your password.";

            await _emailSender.SendEmailAsync(user.Email, "Password Reset Request", message);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // GET: /Auth/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            var resetToken = _context.ResetPasswordToken.FirstOrDefault(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                return RedirectToAction("TokenExpired");
            }

            var model = new PasswordResetModel { Token = token };
            return View(model);
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(PasswordResetModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            // Check the token validity
            var resetToken = _context.ResetPasswordToken.FirstOrDefault(t => t.Token == model.Token && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                return RedirectToAction("TokenExpired");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == resetToken.UserId);

            if (user == null)
            {
                return RedirectToAction("TokenExpired");
            }

           var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);

            _context.ResetPasswordToken.Remove(resetToken);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Auth");
        }

        public IActionResult TokenExpired()
        {
            return View();
        }
    }   
}
