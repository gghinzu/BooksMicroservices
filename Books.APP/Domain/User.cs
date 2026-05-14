using System.ComponentModel.DataAnnotations;
using CORE.APP.Domain;

namespace Books.APP.Domain
{
    public class User : Entity
    {
        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}