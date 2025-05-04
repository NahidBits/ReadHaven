using System.Numerics;
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
    [Route("[controller]")]
    public class AuthController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly CartService _cartService;

        public AuthController(AppDbContext context, IEmailSender emailSender, IConfiguration configuration, IUserService userService, CartService cartService)
        {
            _context = context;
            _emailSender = emailSender;
            _configuration = configuration;
            _userService = userService;
            _cartService = cartService;
        }

        [HttpGet("Login")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Index(User user)
        {
            if (!ModelState.IsValid)
                return Ok(new { success = false, message = "All fields are required. Please complete the form." });
            else if(!IsValidEmail(user.Email))
            {
                return Ok(new { success = false, message = "The email address format is invalid." });
            }

            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (findUser != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(findUser, findUser.PasswordHash, user.PasswordHash);

                if (result == PasswordVerificationResult.Success)
                {
                    var findUserRole = await _context.UserRoles.FirstOrDefaultAsync(u => u.UserId == findUser.Id);
                    if (findUserRole == null) return View();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, findUser.Id.ToString()),
                        new Claim(ClaimTypes.Email, findUser.Email),
                        new Claim(ClaimTypes.Role, findUserRole.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    MergeGuestSessionToUser(findUser.Id);

                    return Ok(new { success = true });
                }
            }


            return Ok(new { success = false, message = "Invalid email or password." });
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(User user, string confirmPassword)
        {
            if (!ModelState.IsValid || user.Username==null)
                return Ok(new { success = false, message = "All fields are required.Please complete the form."});
            else if (!IsValidEmail(user.Email))
            {
                return Ok(new { success = false, message = "The email address format is invalid." });
            }
            else if (!IsValidPassword(user.PasswordHash))
                return Ok(new { success = false, message = "Password must be at least 8 characters long and contain a mix of uppercase, lowercase, number, and special character." });
            else if (user.PasswordHash != confirmPassword)
                return Ok(new { success = false, message = "Password and Confirm Password do not match." });


            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (findUser != null || user.PasswordHash != confirmPassword) return View();

                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);

                _context.Users.Add(user);

                var userRole = new UserRole { UserId = user.Id, Role = "User" };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, userRole.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                MergeGuestSessionToUser(user.Id);

            return Ok(new { success = true});
        }

        [HttpGet("GetUserRoleStatus")]
        public IActionResult GetUserRoleStatus()
        {
            return Ok(UserRole);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(!IsValidEmail(email))
            {
                return Ok(new { success = false, message = "Invalid Email." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Ok(new { success = false, message = "user not found" });

            var token = Guid.NewGuid().ToString();
            var resetToken = new ResetPasswordToken
            {
                UserId = user.Id,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddHours(1)
            };

            _context.ResetPasswordToken.Add(resetToken);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Auth", new { token }, Request.Scheme);
            var message = $"Click <a href='{resetLink}'>here</a> to reset your password.";

            await _emailSender.SendEmailAsync(user.Email, "Password Reset Request", message);

            return Ok(new { success = true, message = "Resetlink is sended" });
        }

        [HttpGet("ForgotPasswordConfirmation")]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet("ResetPassword")]  
        public IActionResult ResetPassword(string token)
        {
            var resetToken = _context.ResetPasswordToken
                .FirstOrDefault(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null) return RedirectToAction("TokenExpired");

            var model = new PasswordResetModel { Token = token };
            return View(model);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(PasswordResetModel resetPassword)
        {
            if(!ModelState.IsValid)
                return Ok(new { success = false, message = "Please correct the highlighted errors in the form." });
            else if (resetPassword.ConfirmPassword != resetPassword.NewPassword)
            {
                return Ok(new { success = false, message = "Password doesnt match." });
            }else if(!IsValidPassword(resetPassword.NewPassword))
            {
                return Ok(new { success = false, message = "Password must be at least 8 characters long and contain a mix of uppercase, lowercase, number, and special character." });
            }

            var resetToken = _context.ResetPasswordToken
                .FirstOrDefault(t => t.Token == resetPassword.Token && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null) return RedirectToAction("TokenExpired");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == resetToken.UserId);
            if (user == null) return RedirectToAction("TokenExpired");

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, resetPassword.NewPassword);

            _context.ResetPasswordToken.Remove(resetToken);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("TokenExpired")]
        public IActionResult TokenExpired() => View();
        protected void MergeGuestSessionToUser(Guid userId)
        {
            var guestCart = _cartService.GetCartItemsForGuest();
            foreach (var item in guestCart)
            {
                item.UserId = userId;
                _cartService.AddToCartForUser(item);
            }

            _cartService.ClearAllGuestCart();
        }
    }
}
