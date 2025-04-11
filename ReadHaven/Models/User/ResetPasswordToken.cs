using System.ComponentModel.DataAnnotations;

namespace ReadHaven.Models.User
{
    public class ResetPasswordToken
    {
        [Key]
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Guid UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
