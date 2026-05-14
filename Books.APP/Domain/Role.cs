using System.ComponentModel.DataAnnotations;
using CORE.APP.Domain;

namespace Books.APP.Domain
{
    public class Role : Entity
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}