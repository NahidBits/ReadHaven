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
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Index(User user, string returnUrl = null)
        {
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

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Book");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("SignUp")]
        public IActionResult SignUp(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(User user, string confirmPassword, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
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

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Book");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
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
            //RedirectToAction("ForgotPasswordConfirmation");
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
        public async Task<IActionResult> ResetPassword(PasswordResetModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            var resetToken = _context.ResetPasswordToken
                .FirstOrDefault(t => t.Token == model.Token && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null) return RedirectToAction("TokenExpired");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == resetToken.UserId);
            if (user == null) return RedirectToAction("TokenExpired");

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);

            _context.ResetPasswordToken.Remove(resetToken);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Auth");
        }

        [HttpGet("TokenExpired")]
        public IActionResult TokenExpired() => View();

        private void MergeGuestSessionToUser(Guid userId)
        {
            var guestCart = _cartService.GetCartItemsForGuest();
            foreach (var item in guestCart)
            {
                item.UserId = userId;
                _cartService.AddToCartForUser(item);
            }

            _cartService.ClearAllGuestCart();
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
