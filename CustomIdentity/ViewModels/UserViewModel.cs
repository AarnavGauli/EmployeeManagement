using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.ViewModels
{
    public class UserViewModel
    {
        
        public required string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? CurrentPassword {  get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
        public string? Number {  get; set; }
        public IList<string>? Roles { get; set; }


    }

}
