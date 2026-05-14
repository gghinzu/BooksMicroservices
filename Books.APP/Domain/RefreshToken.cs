using System.ComponentModel.DataAnnotations;
using CORE.APP.Domain;

namespace Books.APP.Domain
{
    public class RefreshToken : Entity
    {
        [Required, StringLength(500)]
        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool IsActive { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}