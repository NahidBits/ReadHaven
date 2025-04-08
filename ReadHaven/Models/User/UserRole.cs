using ReadHaven.Models.Common;

namespace ReadHaven.Models.User
{
    public class UserRole : BaseEntity      
    {    
        public Guid UserId { get; set; }
        public string Role { get; set; }
    }
}
