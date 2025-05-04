using Microsoft.AspNetCore.Mvc;
using ReadHaven.Services;
using System.Security.Claims;

namespace ReadHaven.Controllers
{
    public class BaseController : Controller
    {
        protected Guid UserId
        {
            get
            {
                var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
            }
        }

        protected string UserEmail
        {
            get => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        protected string UserRole
        {
            get => User.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";
        }

        protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        protected bool IsValidEmail(string email)
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
        protected bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            var isLongEnough = password.Length >= 8;

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecial && isLongEnough;
        }
    }
}
