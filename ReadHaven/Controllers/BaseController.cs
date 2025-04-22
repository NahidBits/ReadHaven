using Microsoft.AspNetCore.Mvc;
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
    }
}
