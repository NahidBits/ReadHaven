using Microsoft.AspNetCore.Identity;
using ReadHaven.Models.Common;                                  

namespace ReadHaven.Models.User
{
    public class User : BaseEntity
    {
        public string? Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
