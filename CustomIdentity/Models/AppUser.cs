using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.Models
{
    public class AppUser:IdentityUser
    {
        [MaxLength(100)]
        [Required]
        public string? Name {  get; set; }
        public string? Address { get; set; }
        public string? Number {  get; set; }
        public bool IsDeleted { get; set; }
    }
}
