using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyFirstIdentity.Models
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        [MaxLength(100)]
        [Required]

        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Password { get; set; }
    }
}
